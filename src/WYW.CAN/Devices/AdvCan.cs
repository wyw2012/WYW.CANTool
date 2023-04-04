using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace WYW.CAN
{
    /// <summary>
    /// 研华Can卡，新旧驱动版本的API不一样，该系统会自动识别
    /// </summary>
    public class AdvCan : CanBase
    {
        private IntPtr advDeviceHandle = IntPtr.Zero;
        private IntPtr advReceiveHandle = IntPtr.Zero;
        private IntPtr advConfigHandle = IntPtr.Zero;
        private IntPtr advCommandHandle = IntPtr.Zero;
        private IntPtr advStatusHandle = IntPtr.Zero;


        private int advOutLength = 0;
        private string advCanName = "";

        private readonly int ADV_CONFIG_OR_COMMAND_LENGTH = 24;  // Marshal.SizeOf(typeof(AdvParameter))
        private readonly int ADV_SATUS_LENGTH = 72;   // Marshal.SizeOf(typeof(AdvStatus))
        private readonly int ADV_MESSAGE_LENGTH = 30;   // Marshal.SizeOf(typeof(AdvMessageFrame)) 驱动1.35版本是22字节，1.37版本是30字节
        private readonly int ADV_MAX_READ_FRAME = 100; // 最大接收帧个数
        private readonly bool isNewDriver = true;
        public AdvCan(string canName, int boudRate = 500)
        {
            advCanName = $"\\\\.\\{canName}";
            this.BaudRate = (UInt32)boudRate;
            isNewDriver = IsNewDriver();
            if (!isNewDriver)
            {
                ADV_MESSAGE_LENGTH = 22;
            }
        }
        #region  公共方法
        public override void Open()
        {
            AllocateMemory();
            OpenDevice();
            StopCan();
            InitPara();
            ClearBuffer();
            StartCan();
            IsOpen = true;
            InvokeStatusChangedEvent("打开设备成功");
            new Thread(Receive).Start();
        }
        public override void Close()
        {
            if (advDeviceHandle != IntPtr.Zero)
            {
                AdvApi.CloseHandle(advDeviceHandle);
                Thread.Sleep(100);
                FreeMemory();
                IsOpen = false;
                advDeviceHandle = IntPtr.Zero;
            }


        }
        public override void Reset()
        {

            IntPtr inBuffer = Marshal.AllocHGlobal(ADV_CONFIG_OR_COMMAND_LENGTH);
            Marshal.StructureToPtr(new AdvParameter() { Command = (int)AdvCommand.ResetChip }, inBuffer, true);
            var result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Command, inBuffer, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("CAN卡复位失败");
            }
        }

        public override bool Write(int id, byte[] data, bool isExternFrame, bool isRemoteFrame)
        {
            if (!IsOpen)
            {
                throw new Exception("写数据失败，原因是CAN卡未打开。");
            }
            int flag = 0;
            if (isExternFrame)
            {
                flag = flag | (1 << 2);
            }
            if (isRemoteFrame)
            {
                flag = flag | (1 << 0);
            }
            var result = false;
            if (isNewDriver)
            {
                AdvMessageFrame2[] frame = {
                    new AdvMessageFrame2()
                    {
                        Flag = flag,
                        ID = (UInt32)id,
                        Data = new byte[8]
                    }};
                if (data == null || isRemoteFrame)
                {
                    frame[0].DataLength = 0;
                }
                if (data != null)
                {
                    frame[0].DataLength = (byte)data.Length;
                    for (int i = 0; i < frame[0].DataLength; i++)
                    {
                        frame[0].Data[i] = data[i];
                    }
                }
                result = AdvApi.WriteFile(advDeviceHandle, frame, 1, out _, IntPtr.Zero);
            }
            else
            {
                AdvMessageFrame1[] frame = {
                    new AdvMessageFrame1()
                    {
                        Flag = flag,
                        ID = (UInt32)id,
                        Data = new byte[8]
                    }};
                if (data == null || isRemoteFrame)
                {
                    frame[0].DataLength = 0;
                }
                if (data != null)
                {
                    frame[0].DataLength = (byte)data.Length;
                    for (int i = 0; i < frame[0].DataLength; i++)
                    {
                        frame[0].Data[i] = data[i];
                    }
                }
                result = AdvApi.WriteFile(advDeviceHandle, frame, 1, out _, IntPtr.Zero);
            }
            InvokeDataTransmitEvent(id, data, isExternFrame, isRemoteFrame, result);
            return result;
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 获取设备状态，暂时未使用
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool GetDeviceStatus(ref AdvStatus status)
        {
            Marshal.StructureToPtr(new AdvStatus(), advStatusHandle, true);
            if (!AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Status, advStatusHandle, ADV_SATUS_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero))
            {
                status = (AdvStatus)(Marshal.PtrToStructure(advStatusHandle, typeof(AdvStatus)));
                return true;
            }
            return false;
        }

        private void AllocateMemory()
        {
            advReceiveHandle = Marshal.AllocHGlobal(ADV_MESSAGE_LENGTH * ADV_MAX_READ_FRAME);
            advConfigHandle = Marshal.AllocHGlobal(ADV_CONFIG_OR_COMMAND_LENGTH);
            advCommandHandle = Marshal.AllocHGlobal(ADV_CONFIG_OR_COMMAND_LENGTH);
            advStatusHandle = Marshal.AllocHGlobal(ADV_SATUS_LENGTH);
        }
        private void FreeMemory()
        {
            Marshal.FreeHGlobal(advReceiveHandle);
            Marshal.FreeHGlobal(advConfigHandle);
            Marshal.FreeHGlobal(advCommandHandle);
            Marshal.FreeHGlobal(advStatusHandle);
        }
        private void OpenDevice()
        {
            // 同步模式创建
            advDeviceHandle = AdvApi.CreateFile(advCanName, AdvFileAccess.Read | AdvFileAccess.Write, 0, IntPtr.Zero, 3, (int)AdvTransferMode.Sync, IntPtr.Zero);
            if ((uint)advDeviceHandle == 0xffffffff)
            {
                throw new Exception("打开设备失败");
            }

        }
        private void InitPara()
        {
            bool result = true;
            // Set AccMask:0xFFFFFFFF ,AccCode:0
            Marshal.StructureToPtr(new AdvParameter() { Config = 0, Parameter1 = 0xFFFFFFFF, Parameter2 = 0 }, advConfigHandle, true);
            result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set mask failed");
            }

            // Set BaudRate. 根据构造函数的参数
            Marshal.StructureToPtr(new AdvParameter() { Config = 3, Parameter1 = BaudRate }, advConfigHandle, true);
            result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set baudrate failed");
            }
            // Set Listen Mode. 正常模式
            Marshal.StructureToPtr(new AdvParameter() { Config = 8, Parameter1 = 0 }, advConfigHandle, true);
            result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set listen mode failed");
            }
            // Set Self Accept. 正常模式
            Marshal.StructureToPtr(new AdvParameter() { Config = 9, Parameter1 = 0 }, advConfigHandle, true);
            result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set accept mode failed");
            }
            // Set Timeout
            Marshal.StructureToPtr(new AdvParameter() { Config = 13, Parameter1 = (UInt32)Timeout, Parameter2 = (UInt32)Timeout }, advConfigHandle, true);
            result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set timeout failed");
            }
            // Set Filter. 双滤波
            Marshal.StructureToPtr(new AdvParameter() { Config = 20, Parameter1 = 0 }, advConfigHandle, true);
            AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Config, advConfigHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Set filter failed");
            }
        }

        private void ClearBuffer()
        {
            Marshal.StructureToPtr(new AdvParameter() { Command = (int)AdvCommand.ClearReceiveBuffer }, advCommandHandle, true);
            var result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Command, advCommandHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Clear buffer failed");
            }
        }

        private void StartCan()
        {
            Marshal.StructureToPtr(new AdvParameter() { Command = (int)AdvCommand.StartChip }, advCommandHandle, true);
            var result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Command, advCommandHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                throw new Exception("Start device failed");
            }
        }

        private void StopCan()
        {
            Marshal.StructureToPtr(new AdvParameter() { Command = (int)AdvCommand.StopChip, Parameter1 = 1, Parameter2 = 1 }, advCommandHandle, true);
            var result = AdvApi.DeviceIoControl(advDeviceHandle, AdvControlMode.Command, advCommandHandle, ADV_CONFIG_OR_COMMAND_LENGTH, IntPtr.Zero, 0, ref advOutLength, IntPtr.Zero);
            if (!result)
            {
                IsOpen = false;
                throw new Exception("Stop 失败");
            }
        }

        private void Receive()
        {
            while (IsOpen)
            {
                if (isNewDriver)
                {
                    if (AdvApi.ReadFile(advDeviceHandle, advReceiveHandle, ADV_MAX_READ_FRAME * ADV_MESSAGE_LENGTH, out int outFrameSize, IntPtr.Zero))
                    {
                        int realFrameCount = outFrameSize / ADV_MESSAGE_LENGTH;
                        for (int i = 0; i < realFrameCount; i++)
                        {
                            AdvMessageFrame2 receiveMessage;
                            if (IntPtr.Size == 8)
                            {
                                // 64位操作系统
                                receiveMessage = (AdvMessageFrame2)(Marshal.PtrToStructure(new IntPtr(advReceiveHandle.ToInt64() + ADV_MESSAGE_LENGTH * i), typeof(AdvMessageFrame2)));
                            }
                            else
                            {
                                // 32位操作系统
                                receiveMessage = (AdvMessageFrame2)(Marshal.PtrToStructure(new IntPtr(advReceiveHandle.ToInt32() + ADV_MESSAGE_LENGTH * i), typeof(AdvMessageFrame2)));
                            }

                            var data = new byte[0];
                            if (receiveMessage.DataLength > 0)
                            {
                                data = receiveMessage.Data.Take(receiveMessage.DataLength).ToArray();
                            }
                            InvokeDataReceivedEvent((int)receiveMessage.ID, data, (receiveMessage.Flag & 0x04) == 0x04 ? 1 : 0, (receiveMessage.Flag & 0x01) == 0x01 ? 1 : 0);
                        }
                    }

                }
                else
                {
                    if (AdvApi.ReadFile(advDeviceHandle, advReceiveHandle, ADV_MAX_READ_FRAME, out int outFrameCount, IntPtr.Zero))
                    {
                        int realFrameCount = outFrameCount;
                        for (int i = 0; i < realFrameCount; i++)
                        {
                            AdvMessageFrame1 receiveMessage;
                            if (IntPtr.Size == 8)
                            {
                                // 64位操作系统
                                receiveMessage = (AdvMessageFrame1)(Marshal.PtrToStructure(new IntPtr(advReceiveHandle.ToInt64() + ADV_MESSAGE_LENGTH * i), typeof(AdvMessageFrame1)));
                            }
                            else
                            {
                                // 32位操作系统
                                receiveMessage = (AdvMessageFrame1)(Marshal.PtrToStructure(new IntPtr(advReceiveHandle.ToInt32() + ADV_MESSAGE_LENGTH * i), typeof(AdvMessageFrame1)));
                            }

                            var data = new byte[0];
                            if (receiveMessage.DataLength > 0)
                            {
                                data = receiveMessage.Data.Take(receiveMessage.DataLength).ToArray();
                            }
                            InvokeDataReceivedEvent((int)receiveMessage.ID, data, (receiveMessage.Flag & 0x04) == 0x04 ? 1 : 0, (receiveMessage.Flag & 0x01) == 0x01 ? 1 : 0);
                        }
                    }
                }

                Thread.Sleep(ReceivedInterval);
            }
        }

        private bool IsNewDriver()
        {
            var driverPath1 = Path.Combine(Environment.SystemDirectory, "drivers", "AdvCanBus.sys");
            var driverPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "drivers", "AdvCanBus.sys");
            if (File.Exists(driverPath1))
            {
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(driverPath1);
                if (version.ProductMajorPart < 5)
                {
                    return false;
                }
            }
            if (File.Exists(driverPath2))
            {
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(driverPath2);
                if (version.ProductMajorPart < 5)
                {
                    return false;
                }
            }
            return true;
        }
    }
    #endregion
}

