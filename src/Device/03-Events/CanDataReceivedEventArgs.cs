using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WYW.CANTool
{
    /// <summary>
    /// CAN接收对象
    /// </summary>
    public class CanDataReceivedEventArgs
    {
        protected CanDataReceivedEventArgs()
        {

        }
        public CanDataReceivedEventArgs(int cobID,int externFlag,int remoteFlag)
        {
            CreateTime = DateTime.Now;
            ID = cobID;
            RemoteFlag = remoteFlag;
            ExternFlag = externFlag;
        }
        /// <summary>
        /// 扩展帧标志，0 标准帧；1 扩展帧
        /// </summary>
        public int ExternFlag { get; protected set; }
        /// <summary>
        /// 远程帧标志，0 数据帧；1 远程帧
        /// </summary>
        public int RemoteFlag { get; protected set; }

        /// <summary>
        /// 帧ID
        /// </summary>
        public int ID { get; protected set; }

        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get; internal set; } = new byte[0];

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get;  }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int length = Data == null ? 0 : Data.Length;

            sb.Append($"[{CreateTime:HH:mm:ss.fff}] [Rx] ");
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
                sb.Append("RTR"); //远程帧
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

            return sb.ToString();
        }
    }
}
