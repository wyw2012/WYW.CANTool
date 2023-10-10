using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// CAN 帧结构基类
    /// </summary>
    public class CanFrameBase:ObservableObject
    {
        public CanFrameBase()
        {
                
        }
        public CanFrameBase(int id, byte[] data, int externFlag, int remoteFlag)
        {
            ID = id;
            Data = data;
            RemoteFlag = remoteFlag;
            ExternFlag = externFlag;
        }
        /// <summary>
        /// 扩展帧标志，0 标准帧；1 扩展帧
        /// </summary>
        public int ExternFlag { get;  set; }
        /// <summary>
        /// 远程帧标志，0 数据帧；1 远程帧
        /// </summary>
        public int RemoteFlag { get;  set; }
        /// <summary>
        /// 帧ID
        /// </summary>
        public int ID { get;  set; }
        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get;  set; } = new byte[0];
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; internal set; }=DateTime.Now;


        /// <summary>
        /// 比较两个对象的标识符是否一致
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal bool IsMatch(CanFrameBase obj)
        {
            if (GetTag() == obj.GetTag())
            {
                return true;
            }
            return false;
        }



        #region 保护方法
        /// <summary>
        /// 设置标签，目的是发送和接收标签保持一致
        /// </summary>
        /// <param name="tag"></param>
        protected virtual string GetTag()
        {
            return string.Empty;
        }
        #endregion
    }
}
