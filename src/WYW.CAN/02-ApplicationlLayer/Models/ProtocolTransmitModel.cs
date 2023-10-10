using System;

namespace WYW.CAN
{
    /// <summary>
    /// 协议传输模型
    /// </summary>
    class ProtocolTransmitModel 
    {
        public ProtocolTransmitModel(CanFrameBase protocol)
        {
            SendBody = protocol;
            lastWriteTime = DateTime.Now;
        }
        /// <summary>
        /// 发送或接收对象
        /// </summary>
        public CanFrameBase SendBody { get; }
        public DateTime CreateTime { get; } = DateTime.Now;

        #region 内部属性
        /// <summary>
        /// 最后一次发送数据结束时间
        /// </summary>
        /// 
        private DateTime lastWriteTime;

        internal DateTime LastWriteTime
        {
            get { return lastWriteTime; }
            set
            { 
                lastWriteTime = value;
                SendBody.CreateTime = value; // 将发送对象的创建时间修改为发送时间
            }
        }
        /// <summary>
        /// 是否需要应答
        /// </summary>
        internal bool IsNeedResponse { get; set; } = true;
        internal CanFrameBase ResponseBody { get; set; }
        /// <summary>
        /// 单次发送超时时间，单位ms
        /// </summary>
        internal int ResponseTimeout { get; set; } = 300;
        /// <summary>
        /// 最大发送次数
        /// </summary>
        internal int MaxSendCount { get; set; } = 1;
        /// <summary>
        /// 是否接收到应答
        /// </summary>
        internal bool HasReceiveResponse { get; set; }
        #endregion
    }
}
