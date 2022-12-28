using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WYW.CANTool
{
    /// <summary>
    /// CAN发送对象
    /// </summary>
    public class CanDataTransmitEventArgs
    {

        public CanDataTransmitEventArgs(int cobID, int externFlag, int remoteFlag,bool result)
        {

            CreateTime = DateTime.Now;
            ID = cobID;
            RemoteFlag = remoteFlag;
            ExternFlag = externFlag;
            Result = result;
        }
        /// <summary>
        /// 扩展帧标志，0 标准帧；1 扩展帧
        /// </summary>
        public int ExternFlag { get; }
        /// <summary>
        /// 远程帧标志，0 数据帧；1 远程帧
        /// </summary>
        public int RemoteFlag { get; }

        /// <summary>
        /// 帧ID
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get; internal set; } = new byte[0];

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; }

        /// <summary>
        /// 发送成功标志
        /// </summary>
        public bool Result { get;  }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int length = Data == null ? 0 : Data.Length;


            sb.Append($"[{CreateTime:HH:mm:ss.fff}] [Tx] ");
            if (ExternFlag == 1)
            {
                sb.Append($"ID={ID:X8} ");
            }
            else
            {
                sb.Append($"ID={ID:X3} ");
            }
            if (RemoteFlag == 1)
            {
                sb.Append("RTR");
            }
            else
            {
                sb.Append($"Data[{length}]=");
                sb.Append("{ ");
                if (Data != null)
                {
                    for (int i = 0; i < length; i++)
                    {
                        sb.Append(Data[i].ToString("X2") + " ");
                    }
                }
                sb.Append("}");
            }
            if(Result)
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
