using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
//***************************
//* Can通信基础类
//* 创建者：王彦为
//* 创建时间：2019-06-11
//* 通信流程
//* 1、OpenDevice
//* 2、InitCan
//* 3、StartCan
//* 4、Send or Receive
//***************************
namespace WYW.CAN
{
    public abstract class CanBase: ObservableObject
    {
        public CanBase()
        {
            SetDllDirectory();
        }
        #region 事件
        public delegate void CanReceivedEventHandler(object sender, CanDataReceivedEventArgs e);
        public delegate void CanTransmitEventHandler(object sender, CanDataTransmitEventArgs e);
        public delegate void CanStatusChangedEventHandler(object sender, CanStatusChangedEventArgs e);

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event CanReceivedEventHandler DataReceived;
        /// <summary>
        /// 数据发送事件
        /// </summary>
        public event CanTransmitEventHandler DataTransmited;
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event CanStatusChangedEventHandler StatusChanged;

        #endregion

        #region 属性

        private int receivedInterval = 1;
        private int timeout = 2;
        private bool logEnabled;
        /// <summary>
        /// 设备是否打开
        /// </summary>
        public bool IsOpen { get; protected set; }
        /// <summary>
        /// 波特率，单位kpbs
        /// </summary>
        public UInt32 BaudRate { get; protected set; }

        /// <summary>
        ///  接收线程时间间隔，单位ms
        /// </summary>
        public int ReceivedInterval
        {
            get => receivedInterval;
            set => SetProperty(ref receivedInterval, value);
        }
        /// <summary>
        /// 接收或者发送超时时间，单位ms
        /// </summary>
        public int Timeout
        {
            get => timeout;
            set => SetProperty(ref timeout, value);
        }
        /// <summary>
        /// 是否启用日志
        /// </summary>
        public bool LogEnabled
        {
            get =>  logEnabled;
            set => SetProperty(ref  logEnabled, value);
        }

        #endregion

        #region  公共方法
        /// <summary>
        /// 打开设备
        /// </summary>
        public abstract void Open();
        /// <summary>
        /// 关闭设备
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 复位设备
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="id">帧ID</param>
        /// <param name="data">帧数据，数组长度≤8</param>
        /// <param name="isExternFrame">是否是扩展帧</param>
        /// <param name="isRemoteFrame">是否是远程帧</param>
        /// <returns></returns>
        public abstract bool Write(int id, byte[] data, bool isExternFrame, bool isRemoteFrame);
        #endregion

        #region 保护方法
        
        protected virtual void SetDllDirectory()
        {

        }
        protected void InvokeDataReceivedEvent(int id, byte[] data, int externFlag, int remoteFlag)
        {
            CanDataReceivedEventArgs arg = new CanDataReceivedEventArgs(id, externFlag, remoteFlag)
            {
                Data = data
            };
            if(LogEnabled)
            {
                Logger.WriteLine("CAN",arg.ToString());
            }
            DataReceived?.Invoke(null, arg);
        }
        protected void InvokeDataTransmitEvent(int id, byte[] data, bool isExternFrame, bool isRemoteFrame, bool result)
        {
            CanDataTransmitEventArgs arg = new CanDataTransmitEventArgs(id, isExternFrame ? 1 : 0, isRemoteFrame ? 1 : 0, result)
            {
                Data = data
            };
            if (LogEnabled)
            {
                Logger.WriteLine("CAN", arg.ToString());
            }
            DataTransmited?.Invoke(null, arg);
        }
        protected void InvokeStatusChangedEvent(string message)
        {
            CanStatusChangedEventArgs arg = new CanStatusChangedEventArgs(message);
            if (LogEnabled)
            {
                Logger.WriteLine("CAN", arg.ToString());
            }
            StatusChanged?.Invoke(null, arg);
        }
        #endregion
    }
}
