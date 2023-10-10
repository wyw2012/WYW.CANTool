using System;
using System.Collections.Generic;
using System.IO.Ports;
//*********************************************
//* 创建者：王彦为
//* 创建时间：2020-12-01
//* 描述：使用串口代替CAN，方便测试
//* 协议描述：
//  _______________________________________________
//  |Head  |CobID |Data |
//  |1Byte |4Byte |8Byte|
//  |      |高在前|     |
//  _______________________________________________
//  Head 
//  Bit |7     |6     |5  4|3  2  1  0|
//      |1 扩展|1 远程|保留|帧有效长度|
//      |0 标准|0 数据|置0 |          |

//*********************************************
namespace WYW.CAN
{
    /// <summary>
    /// 使用串口模拟Can信息，用于测试
    /// </summary>
    public class CAN232 : CanBase
    {
        private readonly SerialPort serialPort;

        private readonly int FixLength = 13;
        private List<byte> receiveBuffer = new List<byte>();
        private DateTime lastReceiveTime = DateTime.Now;
        private object receiveLock = new object();
        public CAN232(string portName, int baudRate)
        {
            BaudRate = (UInt32)baudRate;
            serialPort = new SerialPort()
            {
                PortName = portName,
                BaudRate = baudRate,
            };
        }
        #region 公共方法
        public override void Open()
        {
            if (!IsOpen)
            {
                serialPort.DataReceived += OnDataReceived;
                serialPort.Open();
                IsOpen=IsEstablished = true;
                InvokeStatusChangedEvent("打开设备成功");
            }
        }
        public override void Close()
        {
            if (IsOpen)
            {
                serialPort.DataReceived -= OnDataReceived;
                serialPort.Close();
                IsOpen = IsEstablished = false;
                InvokeStatusChangedEvent("关闭设备成功");
            }
        }
        /// <summary>
        /// 无复位功能
        /// </summary>
        public override void Reset()
        {

        }
        // validLength 无意义
        public override bool Write(int id, byte[] data, bool isExternFrame, bool isRemoteFrame)
        {
            byte[] bytes = new byte[FixLength];
            int validLength = data?.Length ?? 0;
            bytes[0] = (byte)((isExternFrame ? 1 << 7 : 0) + (isRemoteFrame ? 1 << 6 : 0) + validLength);
            bytes[1] = (byte)(id >> 24);
            bytes[2] = (byte)(id >> 16);
            bytes[3] = (byte)(id >> 8);
            bytes[4] = (byte)(id);
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    bytes[i + 5] = data[i];
                }
            }
            if (IsOpen)
            {
                serialPort.Write(bytes, 0, FixLength);
                InvokeDataTransmitEvent(id, data, isExternFrame, isRemoteFrame, true);
                if(IsSelfTestMode)
                {
                    InvokeDataReceivedEvent(id, data, isExternFrame?1:0, isRemoteFrame?1:0);
                }
                return true;
            }
            return false;
        }

        #endregion

        #region 私有方法
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            var bytesLength = serialPort.BytesToRead;
            var receivedData = new byte[bytesLength];
            serialPort.Read(receivedData, 0, bytesLength);
            lock (receiveLock)
            {
                if ((DateTime.Now - lastReceiveTime).TotalMilliseconds > 200)
                {
                    receiveBuffer.Clear();
                }
                lastReceiveTime = DateTime.Now;
                receiveBuffer.AddRange(receivedData);
                ProcessReceiveBuffer();
            }

        }

        private void ProcessReceiveBuffer()
        {
            int startIndex = 0;
            for (int i = 0; i < receiveBuffer.Count; i++)
            {
                if (receiveBuffer.Count < FixLength)
                    break;
                startIndex = i;
                // 判断首帧是否满足要求，必须包含长度和远程帧、扩展帧信息
                if ((receiveBuffer[i] & 0x0F) <= 8 && (receiveBuffer[i] & 0x30) == 0)
                {
                    var fullPacket = new byte[FixLength];
                    receiveBuffer.CopyTo(startIndex, fullPacket, 0, FixLength);

                    var externFlag = fullPacket[0] >> 7;
                    var remoteFlag = fullPacket[0] >> 6;
                    var id = fullPacket[4] | (fullPacket[3] << 8) | (fullPacket[2] << 16) | (fullPacket[1] << 24);
                    DataReceivedEventArgs arg = new DataReceivedEventArgs(id, externFlag, remoteFlag);

                    byte[] data = new byte[0];
                    int validLength = fullPacket[0] & 0x0F;
                    if (validLength > 0)
                    {
                        data = fullPacket.SubBytes(5, validLength);
                    }
                    InvokeDataReceivedEvent(id, data, externFlag, remoteFlag);

                }
                receiveBuffer.RemoveRange(startIndex, FixLength);
                i = startIndex - 1; // 减一的目的是保证i=i;

            }
        }

        #endregion
    }
}
