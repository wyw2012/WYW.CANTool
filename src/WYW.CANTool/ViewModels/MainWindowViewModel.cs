using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.IO;
using WYW.UI.Controls;
using MessageBoxImage = WYW.UI.Controls.MessageBoxImage;
using WYW.CANTool.Models;
using System.Collections;
using System.Text;
using WYW.CAN;

namespace WYW.CANTool.ViewModes
{
    class MainWindowViewModel : ObservableObject
    {
        private Thread sendThread = null;
        private static object displayLocker = new object();

        private List<Tuple<int, byte[]>> sendQueue = new List<Tuple<int, byte[]>>(); // 循环发送队列
        public MainWindowViewModel()
        {
            InitCommand();
        }

        #region 属性
        public Config Config { get; } = new Config();
        public string[] PortNames => System.IO.Ports.SerialPort.GetPortNames();
        private bool isOpen;

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpen
        {
            get => isOpen;
            set => SetProperty(ref isOpen, value);
        }

        private CanBase device;

        /// <summary>
        /// CAN设备
        /// </summary>
        public CanBase Device
        {
            get => device;
            set => SetProperty(ref device, value);
        }


        #endregion

        #region 命令
        private void InitCommand()
        {
            OpenCommand = new RelayCommand(Open);
            CloseCommand = new RelayCommand(Close);
            ClearCommand = new RelayCommand(Clear);
            SendCommand = new RelayCommand(Send);
            StopCommand = new RelayCommand(Stop);
            OpenFileCommand = new RelayCommand(OpenFile);
            CopyTextCommand = new RelayCommand<object>(CopyText);

        }

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public RelayCommand OpenFileCommand { get; private set; }
        public RelayCommand<object> CopyTextCommand { get; private set; }
        private void Clear()
        {
            lock (displayLocker)
            {
                Config.Display.DisplayItems.Clear();

            }
            Config.Status.TotalReceived = 0;
            Config.Status.TotalSended = 0;
            Config.Status.Progress = 0;
        }

        private void Open()
        {
            try
            {
                switch (Config.CAN.CanManufacturer)
                {
                    case 0:
                        Device = new ZlgCan((ZlgBoardType)Config.CAN.CanType, Config.CAN.BoardIndex, Config.CAN.CanBaudRate, Config.CAN.ChannelIndex);
                        break;
                    case 1:
                        Device = new GcgdCan((ZlgBoardType)Config.CAN.CanType, Config.CAN.BoardIndex, Config.CAN.CanBaudRate, Config.CAN.ChannelIndex);
                        break;
                    case 2:
                        Device = new CxgdCan((ZlgBoardType)Config.CAN.CanType, Config.CAN.BoardIndex, Config.CAN.CanBaudRate, Config.CAN.ChannelIndex);
                        break;
                    case 3:
                        Device = new ITEKonCan((ZlgBoardType)Config.CAN.CanType, Config.CAN.BoardIndex, Config.CAN.CanBaudRate, Config.CAN.ChannelIndex);
                        break;
                    case 4:
                        var driverPath1 = Path.Combine(Environment.SystemDirectory, "drivers", "AdvCanBus.sys");
                        var driverPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "drivers", "AdvCanBus.sys");
                        if (!File.Exists(driverPath1) && !File.Exists(driverPath2))
                        {
                            MessageBoxWindow.Show("驱动未安装，请安装后重试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        Device = new AdvCan($"can{Config.CAN.ChannelIndex + 1}", Config.CAN.CanBaudRate);
                        break;
                    case 5:
                        Device = new CAN232(Config.CAN.PortName, Config.CAN.SerialBaudRate);
                        break;
                }
                Clear();
                Device.DataReceived += CAN_DataReceived;
                Device.DataTransmited += CAN_DataTransmit;
                Device.StatusChanged += CAN_StatusChanged;

                Device.Open();
                IsOpen = true;
            }
            catch (Exception ex)
            {
                Device = null;
                MessageBoxWindow.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Close()
        {
            if (device != null)
            {
                try
                {
                    Stop();
                    device.Close();
                    device.DataReceived -= CAN_DataReceived;
                    device.DataTransmited -= CAN_DataTransmit;
                    device.StatusChanged -= CAN_StatusChanged;

                    device = null;
                    IsOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Send()
        {
            try
            {
                CheckSendText();
                sendThread = new Thread(() =>
                {
                    Config.Status.IsSending = true;
                    do
                    {
                        if (!IsOpen || !Config.Status.IsSending)
                            break;
                        Config.Status.Progress = 0;
                        for (int i = 0; i < sendQueue.Count; i++)
                        {
                            if (!IsOpen || !Config.Status.IsSending)
                                break;
                            device.Write(sendQueue[i].Item1, sendQueue[i].Item2, Config.Send.ExtendFrame == 1, Config.Send.RemoteFrame == 1);
                            Config.Status.Progress = ((i + 1) * 100) / sendQueue.Count;
                            if (i != sendQueue.Count - 1 || Config.Send.IsCyclic)
                            {
                                Thread.Sleep(Config.Send.CyclicInterval);
                            }

                        }
                    } while (Config.Send.IsCyclic);
                    Config.Status.IsSending = false;
                });
                sendThread.Start();
            }
            catch (Exception ex)
            {
                Config.Status.IsSending = false;
                MessageBoxWindow.Warning(ex.Message, "警告");
            }
        }

        private void Stop()
        {
            Config.Status.IsSending = false;
        }
        private void OpenFile()
        {
            // 命令执行在IsChecked之后，所以根据改变后的状态进行判断
            if (!Config.Send.IsSendFile)
            {
                return;
            }
            Config.Send.FilePath = "";
            var ofd = new OpenFileDialog
            {
                Filter = "文本文档|*.txt",
                FilterIndex = 1,
                RestoreDirectory = true,
            };
            if (ofd.ShowDialog() == true)
            {
                Config.Send.FilePath = ofd.FileName;
            }
            else
            {
                Config.Send.IsSendFile = false;
            }
        }
        private void CopyText(object content)
        {
            if (content != null && content is IList items)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in items)
                {
                    sb.AppendLine(item.ToString());
                }
                Clipboard.SetDataObject(sb.ToString());
            }
        }
        #endregion

        #region 事件
        private void CAN_DataTransmit(object sender, CanDataTransmitEventArgs e)
        {
            Config.Status.TotalSended += 1;
            if (!Config.Display.EnableDisplay)
                return;
            lock (displayLocker)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Config.Display.DisplayItems.Count >= Config.Display.MaxItemCount)
                    {
                        Config.Display.DisplayItems.RemoveAt(0);
                    }

                    Config.Display.DisplayItems.Add(e.ToString());
                });
            }
        }
        private void CAN_DataReceived(object sender, CanDataReceivedEventArgs e)
        {
            Config.Status.TotalReceived += 1;
            if (!Config.Display.EnableDisplay)
                return;
            lock (displayLocker)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Config.Display.DisplayItems.Count >= Config.Display.MaxItemCount)
                    {
                        Config.Display.DisplayItems.RemoveAt(0);
                    }

                    Config.Display.DisplayItems.Add(e.ToString());
                });
            }
        }
        private void CAN_StatusChanged(object sender, CanStatusChangedEventArgs e)
        {
            lock (displayLocker)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Config.Display.DisplayItems.Count >= Config.Display.MaxItemCount)
                    {
                        Config.Display.DisplayItems.RemoveAt(0);
                    }

                    Config.Display.DisplayItems.Add(e.ToString());
                });
            }
        }

        #endregion

        #region 私有函数

        private void CheckSendText()
        {
            if (device == null)
            {
                throw new Exception("请先点击“开始”按钮。");
            }
            sendQueue.Clear();
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            if (!Config.Send.IsSendFile)
            {
                items.Add(new Tuple<string, string>(Config.Send.FrameID.TrimAll(), Config.Send.FrameData.TrimAll()));
            }
            else
            {
                var sendTexts = File.ReadLines(Config.Send.FilePath);
                if (sendTexts.Count() == 0)
                {
                    throw new Exception("空文档，请选择正确的文档");
                }
                foreach (var item in sendTexts)
                {
                    var tempArray = item.Replace("，", ",").Split(',');
                    if (tempArray.Length != 2)
                    {
                        throw new Exception("帧ID和数据之前请用逗号隔开");
                    }
                    items.Add(new Tuple<string, string>(tempArray[0].TrimAll(), tempArray[1].TrimAll()));
                }
            }

            int index = 0;
            int id = 0;
            byte[] data = new byte[0];
            string prefix = "";
            foreach (var item in items)
            {
                index++;
                prefix = Config.Send.IsSendFile ? $"第{index}行" : " ";
                if (item.Item1.Length > 8)
                {
                    throw new Exception($"{prefix}帧ID长度应小于等于8。");
                }
                try
                {
                    id = Convert.ToInt32(item.Item1, 16);
                }
                catch
                {
                    throw new Exception($"{prefix}帧ID不是有效的十六进制字符。");
                }
                if (item.Item2.Length % 2 == 1)
                {
                    throw new Exception($"{prefix}帧数据长度应该是偶数。");
                }
                try
                {
                    data = Regex.Split(item.Item2, @"(?<=\G.{2})(?!$)").Select(x => Convert.ToByte(x, 16)).ToArray();
                    if (data.Length > 8)
                    {
                        throw new Exception($"{prefix}帧数据长度应小于等于16。");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{prefix}帧数据不符合十六进制。{ex.Message}");
                }
                sendQueue.Add(new Tuple<int, byte[]>(id, data));
            }
        }
        #endregion
    }
}
