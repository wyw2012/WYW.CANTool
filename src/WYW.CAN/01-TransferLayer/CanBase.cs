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
    public abstract class CanBase : ObservableObject
    {
        public CanBase()
        {
            SetDllDirectory();
        }
        #region 事件
        public delegate void CanReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public delegate void CanTransmitEventHandler(object sender, DataTransmitEventArgs e);
        public delegate void CanStatusChangedEventHandler(object sender, StatusChangedEventArgs e);

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event CanReceivedEventHandler DataReceivedEvent;
        /// <summary>
        /// 数据发送事件
        /// </summary>
        public event CanTransmitEventHandler DataTransmitedEvent;
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event CanStatusChangedEventHandler StatusChangedEvent;

        #endregion

        #region 属性

        private int receivedInterval = 1;
        private int timeout = 2;
        private bool logEnabled;
        private bool isEstablished;
        private bool isOpen;
        /// <summary>
        /// 设备是否打开
        /// </summary>
        /// <summary>
        /// 是否启动，建立连接请使用IsEstablished属性
        /// </summary>
        public bool IsOpen
        {
            get => isOpen;
            protected set => SetProperty(ref isOpen, value);
        }
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
            get => logEnabled;
            set => SetProperty(ref logEnabled, value);
        }
        /// <summary>
        /// 通讯是否建立连接，这里只是传输层建立连接，应用层使用IsConnected
        /// </summary>
        public bool IsEstablished
        {
            get => isEstablished;
            set => SetProperty(ref isEstablished, value);
        }
        /// <summary>
        /// 日志文件夹，绝对或者相对路径
        /// </summary>
        public string LogFolder { get; set; } = "Log\\CAN";

        /// <summary>
        /// 是否是自测模式，自测模式下自发自收
        /// </summary>
        public bool IsSelfTestMode { get; set; }
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
            DataReceivedEventArgs arg = new DataReceivedEventArgs(id, externFlag, remoteFlag)
            {
                Data = data
            };
            if (LogEnabled)
            {
                Logger.WriteLine(LogFolder, arg.ToString());
            }
            DataReceivedEvent?.Invoke(null, arg);
        }
        protected void InvokeDataTransmitEvent(int id, byte[] data, bool isExternFrame, bool isRemoteFrame, bool result)
        {
            DataTransmitEventArgs arg = new DataTransmitEventArgs(id, isExternFrame ? 1 : 0, isRemoteFrame ? 1 : 0, result)
            {
                Data = data
            };
            if (LogEnabled)
            {
                Logger.WriteLine(LogFolder, arg.ToString());
            }
            DataTransmitedEvent?.Invoke(null, arg);
        }
        protected void InvokeStatusChangedEvent(string message)
        {
            StatusChangedEventArgs arg = new StatusChangedEventArgs(message);
            if (LogEnabled)
            {
                Logger.WriteLine(LogFolder, arg.ToString());
            }
            StatusChangedEvent?.Invoke(null, arg);
        }
        #endregion
    }
}
