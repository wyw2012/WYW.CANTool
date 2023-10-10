using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WYW.CAN
{
    /// <summary>
    /// CAN接收对象
    /// </summary>
    public sealed class DataReceivedEventArgs : CanFrameBase
    {
        private DataReceivedEventArgs()
        {

        }
        public DataReceivedEventArgs(int cobID, int externFlag, int remoteFlag)
        {
            ID = cobID;
            RemoteFlag = remoteFlag;
            ExternFlag = externFlag;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int length = Data == null ? 0 : Data.Length;

            sb.Append($"[{CreateTime:HH:mm:ss.fff}] [Rx] ID: {ID.ToString("X8").AddSpace()}, ");

            if (RemoteFlag == 1)
            {
                sb.Append("RTR"); //远程帧
            }
            else
            {
                sb.Append($"Data[{length}]: ");
                if (Data != null)
                {
                    sb.Append(Data.ToHexString());
                }
            }

            return sb.ToString();
        }
    }
}
