using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace WYW.CAN
{
    /// <summary>
    /// 周立功
    /// </summary>
    public class ZlgCan : CanBase
    {
        private ZlgInitParameter initParameter;
        private UInt32 boardType;
        private UInt32 boardIndex;
        private UInt32 channel;
        ZlgMessageFrame[] receiveFrame = new ZlgMessageFrame[100];
        public ZlgCan(ZlgBoardType boardType, int boardIndex = 0, int boudRate = 500, int channel = 0):base()
        {
            this.boardType = (UInt32)boardType;
            this.boardIndex = (UInt32)boardIndex;
            this.channel = (UInt32)channel;
            base.BaudRate = (UInt32)boudRate;
        }
        protected override void SetDllDirectory()
        {
            var bits = IntPtr.Size == 8 ? "x64" : "x86";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lib", "ZLG",bits);
            Environment.CurrentDirectory = path;
        }
        public string Name { get; private set; }
        public string SerialNumber { get; private set; }
        public int ChannelCount { get; private set; }
        public string FirmwareVersion { get; protected set; }
        public string HardwareVersion { get; protected set; }
        public override void Open()
        {
            if (IsOpen)
            {
                Close();
            }
            OpenDevice();
            InitPara();
            ClearBuffer();
            StartCan();
            IsEstablished = IsOpen = true;
            GetBoardInfo();
            InvokeStatusChangedEvent("打开设备成功");
            new Thread(Receive).Start();
        }

        public override void Close()
        {
            if (ZlgApi.VCI_CloseDevice(boardType, boardIndex) == 0)
                throw new Exception("关闭设备失败");
            IsOpen = false;
            InvokeStatusChangedEvent("关闭设备成功");
        }

        public override void Reset()
        {
            if (IsOpen)
            {
                if (ZlgApi.VCI_ResetCAN(boardType, boardIndex, channel) == 0)
                    throw new Exception("复位通道失败");

            }
        }

        public override bool Write(int id, byte[] data, bool isExternFrame, bool isRemoteFrame)
        {
            if (!IsOpen)
            {
                throw new Exception("写数据失败，原因是CAN卡未打开。");
            }

            var remoteFlag = isRemoteFrame ? 1 : 0;
            var externFlag = isExternFrame ? 1 : 0;

            ZlgMessageFrame frame = new ZlgMessageFrame()
            {
                ExternFlag = (byte)externFlag,
                ID = (UInt32)id,
                RemoteFlag = (byte)remoteFlag,
                SendType = IsSelfTestMode?(byte)2:(byte)1,
                Data = new byte[8],

            };
            if (data == null || isRemoteFrame)
            {
                frame.DataLength = 0;
            }
            if (data != null)
            {
                frame.DataLength = (byte)data.Length;
                for (int i = 0; i < frame.DataLength; i++)
                {
                    frame.Data[i] = data[i];
                }
            }
            var result = ZlgApi.VCI_Transmit(boardType, boardIndex, channel, ref frame, 1) == 1;
            InvokeDataTransmitEvent(id, data, isExternFrame, isRemoteFrame, result);
            if (!result)
            {
                GetError();
            }
            return result;
        }

        private void OpenDevice()
        {
            int result = 0;
            result = ZlgApi.VCI_OpenDevice(boardType, boardIndex, 0);
            if (result == -1)
            {
                throw new Exception("未发现CAN卡");
            }
            else if (result == 0)
            {
                throw new Exception("打开设备失败");
            }
        }

        private void InitPara()
        {
            // 初始化参数
            initParameter = new ZlgInitParameter() { AccCode = 0, AccMask = 0xffffffff, Filter = 0, Mode = 0 };
            switch (BaudRate)
            {
                case 5:
                    initParameter.Timing0 = 0xBF;
                    initParameter.Timing1 = 0xFF;
                    break;
                case 10:
                    initParameter.Timing0 = 0x31;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 20:
                    initParameter.Timing0 = 0x18;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 40:
                    initParameter.Timing0 = 0x87;
                    initParameter.Timing1 = 0xFF;
                    break;
                case 50:
                    initParameter.Timing0 = 0x09;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 80:
                    initParameter.Timing0 = 0x83;
                    initParameter.Timing1 = 0xFF;
                    break;
                case 100:
                    initParameter.Timing0 = 0x04;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 125:
                    initParameter.Timing0 = 0x03;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 200:
                    initParameter.Timing0 = 0x81;
                    initParameter.Timing1 = 0xFA;
                    break;
                case 250:
                    initParameter.Timing0 = 0x01;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 400:
                    initParameter.Timing0 = 0x80;
                    initParameter.Timing1 = 0xFA;
                    break;
                case 500:
                    initParameter.Timing0 = 0x00;
                    initParameter.Timing1 = 0x1C;
                    break;
                case 666:
                    initParameter.Timing0 = 0x80;
                    initParameter.Timing1 = 0xB6;
                    break;
                case 800:
                    initParameter.Timing0 = 0x00;
                    initParameter.Timing1 = 0x16;
                    break;
                case 1000:
                    initParameter.Timing0 = 0x00;
                    initParameter.Timing1 = 0x14;
                    break;
                default:
                    throw new Exception("波特率错误");
            }
            if (ZlgApi.VCI_InitCAN(boardType, boardIndex, channel, ref initParameter) == 0)
                throw new Exception($"初始化通道{channel} 失败");
        }

        private void ClearBuffer()
        {
            ZlgApi.VCI_ClearBuffer(boardType, boardIndex, channel);
        }

        private void StartCan()
        {
            var result = ZlgApi.VCI_StartCAN(boardType, boardIndex, channel);
            if (result == 0)
            {
                throw new Exception("启动通道失败");
            }
        }

        private void Receive()
        {
            while (IsOpen)
            {
                try
                {
                    var num = ZlgApi.VCI_GetReceiveNum(boardType, boardIndex, channel);
                    if (num > 0)
                    {
                        // 这里每次收取1帧，防止报内存损坏错误
                        var result = ZlgApi.VCI_Receive(boardType, boardIndex, channel, ref receiveFrame[0], 1, 0);
                        if (result == 0xFFFFFFFF)
                        {
                            GetError();
                        }
                        else if (result > 0)
                    {
                        for (int i = 0; i < result; i++)
                        {
                            var data = new byte[0];
                            if (receiveFrame[i].DataLength > 0)
                            {
                                data = receiveFrame[i].Data.Take(receiveFrame[i].DataLength).ToArray();
                            }
                            InvokeDataReceivedEvent((int)receiveFrame[i].ID, data, receiveFrame[i].ExternFlag, receiveFrame[i].RemoteFlag);
                        }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(ReceivedInterval);
            }
        }

        private void GetBoardInfo()
        {
            if (IsOpen)
            {
                ZlgBoardInformation info = new ZlgBoardInformation();
                ZlgApi.VCI_ReadBoardInfo(boardType, boardIndex, ref info);

                SerialNumber = Encoding.ASCII.GetString(info.SerialNumber).TrimEnd('\0');
                Name = Encoding.ASCII.GetString(info.HardwareType).TrimEnd('\0');
                ChannelCount = info.ChannelCount;
                HardwareVersion = info.HardwareVersion.ToString();
                FirmwareVersion = info.FirmwareVersion.ToString();
            }
            else
            {
                throw new Exception("设备已关闭");
            }
        }
        private void GetError()
        {
            ZlgErrorInformation errorInfo = new ZlgErrorInformation();
            ZlgApi.VCI_ReadErrInfo(boardType, boardIndex, channel, ref errorInfo);
            var message = $"{typeof(Properties.ZlgErrorCode).GetProperties().SingleOrDefault(x => x.Name == $"ER{errorInfo.ErrorCode:X8}")?.GetValue(null, null).ToString()}";
            if (errorInfo.ErrorCode > 0)
            {
                InvokeStatusChangedEvent($"CAN卡错误，错误码： 0x{errorInfo.ErrorCode:X8}  {message}");
                Reset();
            }
        }
    }
}
