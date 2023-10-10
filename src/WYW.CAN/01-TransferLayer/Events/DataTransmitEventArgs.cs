using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WYW.CAN
{
    /// <summary>
    /// CAN发送对象
    /// </summary>
    public sealed class DataTransmitEventArgs: CanFrameBase
    {

        public DataTransmitEventArgs(int cobID, int externFlag, int remoteFlag,bool result)
        {
            ID = cobID;
            RemoteFlag = remoteFlag;
            ExternFlag = externFlag;
            IsWriteSuccess = result;
        }
        /// <summary>
        /// Write成功标志
        /// </summary>
        public bool IsWriteSuccess { get;  }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int length = Data == null ? 0 : Data.Length;


            sb.Append($"[{CreateTime:HH:mm:ss.fff}] [Tx] ID: {ID.ToString("X8").AddSpace()}, ");
            if (RemoteFlag == 1)
            {
                sb.Append("RTR");
            }
            else
            {
                sb.Append($"Data[{length}]: ");
                if (Data != null)
                {
                    sb.Append(Data.ToHexString());
                }
            }
            if(IsWriteSuccess)
            {
                sb.Append(" [True]");
            }
            else
            {
                sb.Append(" [False]");
            }
            return sb.ToString();
        }
    }
}
