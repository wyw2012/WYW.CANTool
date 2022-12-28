using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using WYW.UI.Controls;
using MessageBoxImage = WYW.UI.Controls.MessageBoxImage;

namespace WYW.CANTool.ViewModes
{
    class MainWindowViewModel : ObservableObject
    {

        private bool isKeepThreadAlive = false;
        private string filePath = "";
        private List<Tuple<int, byte[]>> sendQueue = new List<Tuple<int, byte[]>>(); // 循环发送队列
        private ListBox listBox = null;
        public MainWindowViewModel()
        {
            InitCommand();
        }

        #region 属性
        private CanBase can;

        /// <summary>
        /// 
        /// </summary>
        public CanBase CAN
        {
            get => can;
            set => SetProperty(ref can, value);
        }

        #region  CAN配置

        private int canManufacturer=1;
        private int canType = 4;
        private int boardIndex;
        private int channelIndex;
        private int baudRateIndex=2;
        private int extendFrame;
        private int remoteFrame;
        private string idText="0";
        private string dataText="01 02 03 04 05 06 07 08";

        private int portNameIndex;
        /// <summary>
        /// 0 周立功、创芯科技、iTEKon；1 广成科技；2 研华；3 CAN232
        /// </summary>
        public int CanManufacturer
        {
            get => canManufacturer;
            set
            {
                SetProperty(ref canManufacturer, value);
                if(value==3)
                {
                    BaudRateIndex = 0;
                }
               
            }
        }

        /// <summary>
        /// CAN卡型号
        /// </summary>
        public int CanType
        {
            get => canType;
            set => SetProperty(ref canType, value);
        }

        /// <summary>
        /// CAN波特率：0=1000kbps; 1=800kbps, 2=500kbps, 3=250kbps, 4=125kbps, 5=100kbps, 6=50kbps, 7=20kbps, 8=10kbps, 9=5kbps
        /// 串口波特率：0=9600, 1=19200, 2=38400, 3=57600, 4=115200 
        /// </summary>
        public int BaudRateIndex
        {
            get => baudRateIndex;
            set => SetProperty(ref baudRateIndex, value);
        }


        /// <summary>
        /// 
        /// </summary>
        public int ChannelIndex
        {
            get => channelIndex;
            set => SetProperty(ref channelIndex, value);
        }
        /// <summary>
        /// 
        /// </summary>
        public int BoardIndex
        {
            get => boardIndex;
            set => SetProperty(ref boardIndex, value);
        }

        /// <summary>
        /// 0 标注帧；1 扩展帧
        /// </summary>
        public int ExtendFrame
        {
            get => extendFrame;
            set => SetProperty(ref extendFrame, value);
        }

        /// <summary>
        /// 0 数据帧；1 远程帧
        /// </summary>
        public int RemoteFrame
        {
            get => remoteFrame;
            set => SetProperty(ref remoteFrame, value);
        }

        /// <summary>
        /// 十六进制文本格式的ID号，例如FA
        /// </summary>
        public string IDText
        {
            get => idText;
            set => SetProperty(ref idText, value);
        }

        /// <summary>
        /// 十六进制文本格式的数据，例如01 AA
        /// </summary>
        public string DataText
        {
            get => dataText;
            set => SetProperty(ref dataText, value);
        }


        public string[] PortNames => System.IO.Ports.SerialPort.GetPortNames();
        public int[] CanBaudRateItems => new int[] { 1000, 800, 500, 250, 125, 100, 50, 20, 10, 5 };
        public int[] SerialBaudRateItems => new int[] { 9600, 19200, 38400, 57600, 115200 };
        /// <summary>
        /// 串口号索引
        /// </summary>
        public int PortNameIndex
        {
            get => portNameIndex;
            set => SetProperty(ref portNameIndex, value);
        }

       
        #endregion

        #region Common
        private bool isOpened;
        private bool isCircledSend;
        private int circleInterval = 1000;
      
        private int totalReceived;
        private int totalSended;
        private bool isSendFile;
        private double progress;
        private bool sendButtonEnabled=true;
        /// <summary>
        /// 是否定时发送
        /// </summary>
        public bool IsCircledSend
        {
            get { return isCircledSend; }
            set
            {
                if (isCircledSend != value)
                {
                    isCircledSend = value;
                    OnPropertyChanged("IsCircledSend");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSendFile
        {
            get { return isSendFile; }
            set
            {
                if (isSendFile != value)
                {
                    isSendFile = value;
                    OnPropertyChanged("IsSendFile");
                    if(value)
                    {
                        BrowseSendFile();
                    }
                }
            }
        }

        /// <summary>
        /// 循环发送时间间隔，单位ms
        /// </summary>
        public int CircleInterval
        {
            get { return circleInterval; }
            set
            {
                if (circleInterval != value)
                {
                    circleInterval = value;
                    OnPropertyChanged("CircleInterval");
                }
            }
        }




        /// <summary>
        /// 累计接收字节
        /// </summary>
        public int TotalReceived
        {
            get { return totalReceived; }
            set
            {
                if (totalReceived != value)
                {
                    totalReceived = value;
                    OnPropertyChanged("TotalReceived");
                }
            }
        }

        /// <summary>
        /// 积累发送字节
        /// </summary>
        public int TotalSended
        {
            get { return totalSended; }
            set
            {
                if (totalSended != value)
                {
                    totalSended = value;
                    OnPropertyChanged("TotalSended");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsOpened
        {
            get { return isOpened; }
            set
            {
                if (isOpened != value)
                {
                    isOpened = value;
                    OnPropertyChanged("IsOpened");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public double Progress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SendButtonEnabled
        {
            get => sendButtonEnabled;
            set => SetProperty(ref sendButtonEnabled, value);
        }
       public  ObservableCollection<string> DisplayItems { get;  }=new ObservableCollection<string>();
        #endregion
        #endregion

        #region 命令
        private void InitCommand()
        {
            OpenCommand = new RelayCommand<object>(Open);
            CloseCommand = new RelayCommand(Close);
            ClearCommand = new RelayCommand(Clear);
            SendCommand = new RelayCommand(Send);
            StopCommand = new RelayCommand(Stop);
        
        }

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand<object> OpenCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand SendCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }

        private void Clear()
        {
            DisplayItems.Clear();
            TotalReceived = 0;
            TotalSended = 0;
        }

        private void Open(object obj)
        {
            listBox=obj as ListBox;
            try
            {
                switch (CanManufacturer)
                {
                    case 0:
                        CAN = new ZlgCan((ZlgBoardType)CanType, BoardIndex, CanBaudRateItems[BaudRateIndex], ChannelIndex);
                        break;
                    case 1:
                        CAN = new GcgdCan((ZlgBoardType)CanType, BoardIndex, CanBaudRateItems[BaudRateIndex], ChannelIndex);
                        break;
                    case 2:
                        CAN = new CxgdCan((ZlgBoardType)CanType, BoardIndex, CanBaudRateItems[BaudRateIndex], ChannelIndex);
                        break;
                    case 3:
                        CAN = new ITEKonCan((ZlgBoardType)CanType, BoardIndex, CanBaudRateItems[BaudRateIndex], ChannelIndex);
                        break;
                    case 4:
                        var driverPath1 = Path.Combine(Environment.SystemDirectory, "drivers", "AdvCanBus.sys");
                        var driverPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "drivers", "AdvCanBus.sys");
                        if (!File.Exists(driverPath1) && !File.Exists(driverPath2))
                        {
                            MessageBoxWindow.Show("驱动未安装，请安装后重试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        CAN = new AdvCan($"can{ChannelIndex + 1}", CanBaudRateItems[BaudRateIndex]);
                        break;
                    case 5:
                        CAN = new CAN232(PortNames[PortNameIndex], SerialBaudRateItems[BaudRateIndex]);
                        break;
                }
                Clear();
                CAN.DataReceived += CAN_DataReceived;
                CAN.DataTransmit += CAN_DataTransmit;
                CAN.StatusChanged += CAN_StatusChanged;
            
                CAN.Open();
               
                IsOpened = true;
            }
            catch (Exception ex)
            {
                CAN = null;
                MessageBoxWindow.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void Close()
        {
            if (CAN != null)
            {
                try
                {
                    if (IsCircledSend)
                    {
                        IsCircledSend = false;
                        Thread.Sleep(1000);
                    }
                    CAN.DataReceived -= CAN_DataReceived;
                    CAN.DataTransmit -= CAN_DataTransmit;
                    CAN.StatusChanged -= CAN_StatusChanged;

                    CAN.Close();
                    CAN = null;
                    IsOpened = false;
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
                sendQueue.Clear();
                if (IsSendFile)
                {
                    CheckSendFile(out sendQueue);
                }
                else
                {
                    CheckSendText(out sendQueue);
                }
                SendButtonEnabled = false;
                Progress = 0;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    isKeepThreadAlive = true;
                    SendButtonEnabled = false;
                    do
                    {
                        if (!IsOpened || !isKeepThreadAlive)
                            break;
                        Progress = 0;
                        for (int i = 0; i < sendQueue.Count; i++)
                        {

                            if (!IsOpened || !isKeepThreadAlive)
                                break;
                            CAN.Write(sendQueue[i].Item1, sendQueue[i].Item2, ExtendFrame == 1, RemoteFrame == 1);
                            Progress = ((i + 1) * 100) / sendQueue.Count;
                            if (i != sendQueue.Count - 1 || isCircledSend)
                            {
                                Thread.Sleep(CircleInterval);
                            }
                        }
                    } while (isCircledSend);
                    SendButtonEnabled = true;
                });
            }
            catch (Exception ex)
            {
                SendButtonEnabled = true;
                MessageBoxWindow.Show(ex.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
           
        }

        private void Stop()
        {
            isKeepThreadAlive = false;
        }
        #endregion

        #region 事件
        private void CAN_DataTransmit(object sender, CanDataTransmitEventArgs e)
        {
            TotalSended += 1;
            DisplayToListBox(sender, e);
        }
        private void CAN_DataReceived(object sender, CanDataReceivedEventArgs e)
        {
            TotalReceived += 1;
            DisplayToListBox(sender, e);
        }
        private void CAN_StatusChanged(object sender, CanStatusChangedEventArgs e)
        {
            DisplayToListBox(sender, e);
        }
        private void DisplayToListBox(object sender, object e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                if (DisplayItems.Count > 1000)
                {
                    DisplayItems.RemoveAt(0);
                }
                DisplayItems.Add(e.ToString());
            }));
        }
        #endregion

        #region 私有函数
        private void BrowseSendFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "文本文档|*.txt",
                FilterIndex = 1,
                RestoreDirectory = true,
            };
            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
            }
            else
            {
                IsSendFile = false;
            }
        }

        private void CheckSendText(out List<Tuple<int, byte[]>> list)
        {
            if (CAN == null)
            {
                throw new Exception("请先点击“开始”按钮。");
            }
            list = new List<Tuple<int, byte[]>>();
            var id = 0;
            var buffer = new byte[0];
            try
            {
                id = Convert.ToInt32(IDText, 16);
            }
            catch
            {
                throw new Exception("帧ID不符合字符要求。");
            }
            if (!string.IsNullOrEmpty(DataText))
            {
                var chars = Regex.Replace(DataText, @"\s", ""); // 剔除空格
                if (chars.Length % 2 == 1)
                {
                    throw new Exception("字符格式不符合十六进制。");
                }
                var hexs = Regex.Split(chars, @"(?<=\G.{2})(?!$)");   // 两两分组
                try
                {
                    buffer = hexs.Select(item => Convert.ToByte(item, 16)).ToArray();
                    if (buffer.Length > 8)
                    {
                        throw new Exception($"数据长度大于8个字节。");
                    }
                }
                catch
                {
                    throw new Exception("字符格式不符合十六进制。");
                }
            }
            list.Add(new Tuple<int, byte[]>(id, buffer));
        }

        private void CheckSendFile(out List<Tuple<int, byte[]>> list)
        {
            if (CAN == null)
            {
                throw new Exception("请先点击“开始”按钮。");
            }
            list = new List<Tuple<int, byte[]>>();
            var sendTexts = File.ReadLines(filePath);

            int index = 0;
            foreach (var item in sendTexts)
            {
                index++;
                int id = 0;
                byte[] data = new byte[0];
                var tempArray = item.Split(',');
                if (tempArray.Length != 2)
                {
                    throw new Exception("帧ID和数据之前请用逗号隔开");
                }
                var idText = Regex.Replace(tempArray[0], @"\s", ""); // 剔除空格
                try
                {
                    id = Convert.ToInt32(idText, 16);
                }
                catch
                {
                    throw new Exception($"第{index}行帧ID不符合十六进制。");
                }
                var dataText = Regex.Replace(tempArray[1], @"\s", ""); // 剔除空格
                if (dataText.Length % 2 == 1)
                {
                    throw new Exception($"第{index}行帧数据不符合十六进制。");
                }
                try
                {
                    data = Regex.Split(dataText, @"(?<=\G.{2})(?!$)").Select(x => Convert.ToByte(x, 16)).ToArray();
                    if (data.Length > 8)
                    {
                        throw new Exception($"第{index}行数据长度大于8个字节。");
                    }
                }
                catch
                {
                    throw new Exception($"第{index}行数据不符合十六进制。");
                }
                list.Add(new Tuple<int, byte[]>(id, data));
            }
        }

        #endregion
    }
}
