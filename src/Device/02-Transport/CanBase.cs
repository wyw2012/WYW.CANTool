using System;
using System.Collections.ObjectModel;
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
namespace WYW.CANTool
{
    public abstract class CanBase
    {
        public CanBase()
        {
            SetDllDirectory();
        }
        #region 事件
        public delegate void CanReceivedEventHandler(object sender, CanDataReceivedEventArgs e);
        public event CanReceivedEventHandler DataReceived;

        public delegate void CanTransmitEventHandler(object sender, CanDataTransmitEventArgs e);
        public event CanTransmitEventHandler DataTransmit;

        public delegate void CanStatusChangedEventHandler(object sender, CanStatusChangedEventArgs e);
        public event CanStatusChangedEventHandler StatusChanged;

        #endregion

        /// <summary>
        /// 接收线程时间间隔，单位ms
        /// </summary>
        public int ReceivedInterval { get; set; } = 1;
        public bool IsOpen { get; set; }
        public UInt32 BaudRate { get; set; }
        /// <summary>
        /// 接收或者发送超时时间
        /// </summary>
        public uint Timeout { get; set; } = 2;

        public abstract void Open();
        public abstract void Close();
        public abstract void Reset();
        public abstract bool Write(int id, byte[] data, bool isExternFrame, bool isRemoteFrame);
        protected virtual void SetDllDirectory()
        {

        }

        protected void InvokeDataReceivedEvent(int id, byte[] data, int externFlag, int remoteFlag)
        {
            CanDataReceivedEventArgs arg = new CanDataReceivedEventArgs(id, externFlag, remoteFlag)
            {
                Data = data
            };
            Task.Run(delegate
            {
                DataReceived?.Invoke(null, arg);
            });
        }
        protected void InvokeDataTransmitEvent(int id, byte[] data,bool isExternFrame, bool isRemoteFrame,bool result)
        {
            CanDataTransmitEventArgs arg = new CanDataTransmitEventArgs(id, isExternFrame ? 1 : 0, isRemoteFrame ? 1 : 0, result)
            {
                Data = data
            };
            Task.Run(delegate
            {
                DataTransmit?.Invoke(null, arg);
            });
        }
        protected void InvokeStatusChangedEvent(string message)
        {
            CanStatusChangedEventArgs arg = new CanStatusChangedEventArgs(message);

            Task.Run(delegate
            {
                StatusChanged?.Invoke(null, arg);
            });
        }
    }
}
