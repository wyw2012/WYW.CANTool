using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WYW.CAN
{
    public class Device : ObservableObject, IDisposable
    {
        private bool disposed = false;
        private bool isOpened = false;
        private Thread sendThread = null, heartbeatThread = null;
        private bool isKeepSendThreadAlive = true, isKeepHeartbeatTheadAlive = true;
        private ConcurrentQueue<ProtocolTransmitModel> SendQueue = new ConcurrentQueue<ProtocolTransmitModel>();
        private ProtocolTransmitModel lastSendNeedResponse = null;


        #region 构造函数
        public Device(CanBase client)
        {
            Client = client;
            Client.PropertyChanged += Client_PropertyChanged;
        }

        #endregion

        #region  属性
        private bool isConnected;
        /// <summary>
        /// 是否建立连接，如果有心跳，则利用心跳判断，如果无心跳，则以通讯建立连接为标志
        /// </summary>
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                SetProperty(ref isConnected, value);
            }
        }

        public CanBase Client { get; }

        /// <summary>
        /// 心跳配置
        /// </summary>
        public Heartbeat Heartbeat { get; } = new Heartbeat();

        /// <summary>
        /// 是否为调试模式，调试模式下发送返回成功，但Response值为null，需要调用者继续处理
        /// </summary>
        public bool IsDebugModel { get; set; }

        /// <summary>
        /// 最后一次接收到数据时间
        /// </summary>
        public DateTime LastReceiveTime { get; protected set; } = DateTime.MinValue;
        #endregion

        #region 事件
        public delegate void HeartbeatTriggeredEventHandler(object sender, ExecutionResult result);


        /// <summary>
        /// 心跳触发事件，通过IsSuccess判断心跳是否正常
        /// </summary>
        public event HeartbeatTriggeredEventHandler HeartbeatTriggeredEvent;
        #endregion

        #region  公共方法
        /// <summary>
        /// 打开设备
        /// </summary>
        public void Open()
        {
            if (IsDebugModel)
            {
                IsConnected = true;
                return;
            }

            if (isOpened)
            {
                return;
            }
            if (Client != null)
            {
                Client.Open();
                Client.DataReceivedEvent += Client_DataReceivedEvent;
                isOpened = true;
                heartbeatThread = new Thread(StartHeartbeat) { IsBackground = true };
                heartbeatThread.Start();
                sendThread = new Thread(ProcessSendQueue) { IsBackground = true };
                sendThread.Start();
            }
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        public void Close()
        {
            if (IsDebugModel)
            {
                IsConnected = false;
                return;
            }

            if (!isOpened)
            {
                return;
            }
            if (Client != null)
            {
                Client.DataReceivedEvent -= Client_DataReceivedEvent;
                Client.Close();
                isOpened = false;
                IsConnected = false;
                isKeepHeartbeatTheadAlive = false;
                heartbeatThread = null;
                isKeepSendThreadAlive = false;
                sendThread = null;
            }
        }

        /// <summary>
        /// 释放资源，取消强事件订阅
        /// </summary>
        public void Dispose()
        {
            this.Close();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 保护方法
        protected virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
        {

        }
        protected virtual void OnHeartbeatTriggered(ExecutionResult result)
        {
            Task.Run(() =>
            {
                HeartbeatTriggeredEvent?.Invoke(this, result);
            });
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Client.PropertyChanged -= Client_PropertyChanged;
                }
                disposed = true;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="frame">发送对象</param>
        /// <param name="isNeedResponse">是否需要应答</param>
        /// <param name="maxSendCount">最大发送次数</param>
        /// <param name="responseTimeout">单次发送超时时间，单位ms</param>
        /// <returns>通过IsSuccess判断是否发送成功，Response返回应答数据</returns>
        public ExecutionResult SendProtocol(CanFrameBase frame, bool isNeedResponse = true, int maxSendCount = 1, int responseTimeout = 300)
        {
            if (IsDebugModel)
            {
                return ExecutionResult.Success(null);
            }
            if(!Client.IsOpen)
            {
                return ExecutionResult.Failed("Device is closed. ");
            }
            ProtocolTransmitModel arg = new ProtocolTransmitModel(frame)
            {
                MaxSendCount = maxSendCount,
                ResponseTimeout = responseTimeout,
                IsNeedResponse = isNeedResponse,
            };

            SendQueue.Enqueue(arg);
            CheckSendThread();
            if (arg.IsNeedResponse)
            {
                while (!arg.HasReceiveResponse)
                {
                    // 超时退出
                    if ((DateTime.Now - arg.CreateTime).TotalMilliseconds > arg.MaxSendCount * arg.ResponseTimeout)
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }
                if (arg.HasReceiveResponse)
                {
                    return ExecutionResult.Success(arg.ResponseBody);
                }
                else
                {
                    return ExecutionResult.Failed("Communication timeout");
                }

            }
            return ExecutionResult.Success(null);
        }
        #endregion

        #region 私有方法

        #region 处理发送

        private void ProcessSendQueue()
        {
            isKeepSendThreadAlive = true;
            while (isKeepSendThreadAlive)
            {
                if (SendQueue.Count > 0)
                {

                    var cmd = SendQueue.FirstOrDefault();
                    if (cmd != null)
                    {
                        // 如果排队的时间已经超过最大的超时时间，则从发送队列移除
                        if ((DateTime.Now - cmd.CreateTime).TotalMilliseconds > cmd.MaxSendCount * cmd.ResponseTimeout)
                        {
                            SendQueue.TryDequeue(out _);
                            continue;
                        }
                        if (cmd.IsNeedResponse)
                        {
                            lastSendNeedResponse = cmd;
                            for (int i = 0; i < cmd.MaxSendCount; i++)
                            {
                                Write(cmd);
                                while (!cmd.HasReceiveResponse)
                                {
                                    if ((DateTime.Now - cmd.LastWriteTime).TotalMilliseconds >= cmd.ResponseTimeout)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1);
                                }
                                if (cmd.HasReceiveResponse)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Write(cmd);
                        }
                        SendQueue.TryDequeue(out _);
                    }
                }
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 检查发送线程，如果发送线程意外终止，则重启发送线程
        /// </summary>
        private void CheckSendThread()
        {
            // 如果线程异常死掉
            if (sendThread != null && !sendThread.IsAlive)
            {
                isKeepSendThreadAlive = false;
                sendThread = null;
                sendThread = new Thread(ProcessSendQueue) { IsBackground = true };
                sendThread.Start();
            }
        }
        private void Write(ProtocolTransmitModel arg)
        {
            try
            {
                // 建立连接后才发送
                if (Client.IsEstablished)
                {
                    arg.LastWriteTime = DateTime.Now;
                    Client.Write(arg.SendBody.ID, arg.SendBody.Data, arg.SendBody.ExternFlag == 1, arg.SendBody.RemoteFlag == 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        #endregion

        #region 处理接收

        private void Client_DataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            IsConnected = true;
            LastReceiveTime=DateTime.Now;
            Task.Run(()=>
            {
                OnDataReceived(sender, e);
            });
            if (lastSendNeedResponse != null && !lastSendNeedResponse.HasReceiveResponse &&
                (DateTime.Now - lastSendNeedResponse.LastWriteTime).TotalMilliseconds <= lastSendNeedResponse.ResponseTimeout)
            {
                Type type = lastSendNeedResponse.SendBody.GetType();
                var obj = Activator.CreateInstance(type, e.ID, e.Data, e.ExternFlag, e.RemoteFlag) as CanFrameBase;

                // 判断发送和接收的标识符是否一致，如果一致则认为发送成功
                if (lastSendNeedResponse.SendBody.IsMatch(obj))
                {
                    lastSendNeedResponse.HasReceiveResponse = true;
                    lastSendNeedResponse.ResponseBody = obj;
                }
            }
        }
        #endregion

        #region 心跳
        /// <summary>
        /// 心跳线程
        /// </summary>
        private void StartHeartbeat()
        {
            isKeepHeartbeatTheadAlive = true;
            while (isKeepHeartbeatTheadAlive)
            {
                if (Heartbeat.Content == null || !Heartbeat.IsEnabled)
                {
                    Thread.Sleep(2000);
                    continue;
                }
                if (!Client.IsEstablished)
                {
                    Thread.Sleep(2000);
                    continue;
                }
                if ((DateTime.Now - LastReceiveTime).TotalSeconds >= Heartbeat.IntervalSeconds)
                {
                    var result = SendProtocol(Heartbeat.Content, true, Heartbeat.MaxRetryCount, Heartbeat.Timeout);
                    IsConnected = result.IsSuccess;
                    OnHeartbeatTriggered(result);
                }
                Thread.Sleep(2000);
            }
        }
        #endregion

        #endregion

        #region 事件处理

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!Heartbeat.IsEnabled || Heartbeat.Content == null)
            {
                // 握手信息改变，则连接状态也同步改变
                IsConnected = Client.IsEstablished;
            }
        }
        #endregion

    }
}
