using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using WYW.CommunicationtTool.ViewModes;
using System.Diagnostics;
using WYW.CommunicationtTool.CAN;
using System.Collections.Generic;

namespace WYW.CommunicationtTool.Views
{

    public partial class CanWindow : UserControl
    {
        private CanWindowViewMode viewMode = new CanWindowViewMode();
        private CanBase device = null;
        private int[] canBaudRateArray = new int[] { 1000, 800, 500, 250, 125, 100, 50, 20, 10, 5 };
        private int[] serialBaudRateArray = new int[] { 9600, 19200, 38400, 57600, 115200 };
        private bool isFirstLoad = true;
        private string filePath = "";

        private System.Timers.Timer timer = new System.Timers.Timer();
        private List<Tuple<int, byte[]>> sendQueue = new List<Tuple<int, byte[]>>(); // 循环发送队列
        private int sendIndex = 0;
        public CanWindow()
        {
            InitializeComponent();
            this.DataContext = viewMode;
            timer.Elapsed += Timer_Elapsed;

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoad)
            {
                var portNames = System.IO.Ports.SerialPort.GetPortNames();
                cmbPortName.ItemsSource = portNames;
                if (portNames.Length > 0)
                {
                    cmbPortName.SelectedIndex = 0;
                }
                isFirstLoad = false;
            }

        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int index = (sendIndex++) % sendQueue.Count;
            device.Write(sendQueue[index].Item1, sendQueue[index].Item2, viewMode.ExtendFrame == 1, viewMode.RemoteFrame == 1);
        }
        private void Device_DataTransmit(object sender, CanDataTransmitEventArgs e)
        {
            viewMode.TotalSended += 1;
            Dispatcher.Invoke(new Action(delegate
            {
                if(txtShow.LineCount>1000)
                {
                    txtShow.Clear();
                }
                txtShow.AppendText($"{e}\r\n");
                // 如果滑块在最下面或者最上面，自动滚屏
                if (Math.Abs(txtShow.VerticalOffset + txtShow.ViewportHeight - txtShow.ExtentHeight) <= 1 ||
                    txtShow.VerticalOffset == 0)
                {
                    txtShow.ScrollToEnd();
                }
            }));
        }
        private void Device_DataReceived(object sender, CanDataReceivedEventArgs e)
        {
            viewMode.TotalReceived += 1;
            Console.WriteLine(e);
            Dispatcher.Invoke(new Action(delegate
            {
                txtShow.AppendText($"{e}\r\n");
                // 如果滑块在最下面或者最上面，自动滚屏
                if (Math.Abs(txtShow.VerticalOffset + txtShow.ViewportHeight - txtShow.ExtentHeight) <= 1 ||
                    txtShow.VerticalOffset == 0)
                {
                    txtShow.ScrollToEnd();
                }
            }));
        }
        private void Device_StatusChanged(object sender, CanStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                txtShow.AppendText($"{e}\r\n");
                // 如果滑块在最下面或者最上面，自动滚屏
                if (Math.Abs(txtShow.VerticalOffset + txtShow.ViewportHeight - txtShow.ExtentHeight) <= 1 ||
                    txtShow.VerticalOffset == 0)
                {
                    txtShow.ScrollToEnd();
                }
            }));
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewMode.TotalReceived = 0;
                viewMode.TotalSended = 0;
                switch (viewMode.CanManufacturer)
                {
                    case 0:
                        device = new ZlgCan((ZlgBoardType)viewMode.CanType, viewMode.BoardIndex, canBaudRateArray[viewMode.BaudRate], viewMode.ChannelIndex);
                        break;
                    case 1:
                        device = new GcgdCan((ZlgBoardType)viewMode.CanType, viewMode.BoardIndex, canBaudRateArray[viewMode.BaudRate], viewMode.ChannelIndex);
                        break;
                    case 2:
                        device = new CxgdCan((ZlgBoardType)viewMode.CanType, viewMode.BoardIndex, canBaudRateArray[viewMode.BaudRate], viewMode.ChannelIndex);
                        break;
                    case 3:
                        device = new ITEKonCan((ZlgBoardType)viewMode.CanType, viewMode.BoardIndex, canBaudRateArray[viewMode.BaudRate], viewMode.ChannelIndex);
                        break;
                    case 4:
                        var driverPath1 = Path.Combine(Environment.SystemDirectory, "drivers", "AdvCanBus.sys");
                        var driverPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "drivers", "AdvCanBus.sys");
                        if (!File.Exists(driverPath1) && !File.Exists(driverPath2))
                        {
                            MessageBox.Show("驱动未安装，请安装后重试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        device = new AdvCan($"can{viewMode.ChannelIndex + 1}", canBaudRateArray[viewMode.BaudRate]);
                        break;
                    case 5:
                        device = new CAN232(viewMode.PortName, serialBaudRateArray[viewMode.BaudRate]);
                        break;
                }
                btnClear_Click(null, null);
                device.DataReceived += Device_DataReceived;
                device.DataTransmit += Device_DataTransmit;
                device.StatusChanged += Device_StatusChanged;
                device.Open();
                viewMode.IsOpened = true;
                btnStart.IsEnabled = false;
            }
            catch (Exception ex)
            {
                device = null;
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (device != null)
            {
                try
                {
                    if (viewMode.IsCircledSend)
                    {
                        viewMode.IsCircledSend = false;
                        Thread.Sleep(1000);
                    }
                    device.Close();
                    device.DataReceived -= Device_DataReceived;
                    device.DataTransmit -= Device_DataTransmit;
                    device.StatusChanged -= Device_StatusChanged;
                    device = null;
                    viewMode.IsOpened = false;
                    btnStart.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtShow.Text))
            {
                MessageBox.Show("接收栏无信息，不需要保存。", "提示");
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "Log File|*.txt",
                FilterIndex = 1,
                InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory),
                FileName = $"{DateTime.Now:yyyy-MM-dd}",
                RestoreDirectory = true,
            };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, txtShow.Text);
                MessageBox.Show("保存成功", "提示");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtShow.Clear();
            viewMode.TotalReceived = 0;
            viewMode.TotalSended = 0;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                sendQueue.Clear();
                if (viewMode.IsSendFile)
                {
                    CheckSendFile(out sendQueue);
                }
                else
                {
                    CheckSendText(out sendQueue);
                }
                chbCircleSend.IsEnabled = btnSend.IsEnabled = false;
                progress1.Visibility = Visibility.Visible;
                progress1.Value = 0;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    for (int i = 0; i < sendQueue.Count; i++)
                    {
                        if (!viewMode.IsOpened)
                            break;
                        device.Write(sendQueue[i].Item1, sendQueue[i].Item2, viewMode.ExtendFrame == 1, viewMode.RemoteFrame == 1);
                        viewMode.Progress = ((i + 1) * 100) / sendQueue.Count;
                        if (i == sendQueue.Count - 1)
                            break;
                        Thread.Sleep(viewMode.CircleInterval);
                    }
                    Dispatcher.Invoke(delegate
                    {
                        chbCircleSend.IsEnabled = btnSend.IsEnabled = true;
                        progress1.Visibility = Visibility.Collapsed;
                    });
                });
            }
            catch (Exception ex)
            {
                chbCircleSend.IsEnabled = btnSend.IsEnabled = true;
                progress1.Visibility = Visibility.Collapsed;
                MessageBox.Show(ex.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void chbCircleSend_Click(object sender, RoutedEventArgs e)
        {
            if (viewMode.IsCircledSend)
            {
                try
                {
                    sendQueue.Clear();
                    if (viewMode.IsSendFile)
                    {
                        CheckSendFile(out sendQueue);
                    }
                    else
                    {
                        CheckSendText(out sendQueue);
                    }
                    txtID.IsEnabled = txtData.IsEnabled = false;
                    btnSend.IsEnabled = false;
                    sendIndex = 0;
                    timer.Interval = viewMode.CircleInterval;
                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    viewMode.IsCircledSend = false;
                }
            }
            else
            {
                if (!viewMode.IsSendFile)
                {
                    txtID.IsEnabled = txtData.IsEnabled = true;
                }
                btnSend.IsEnabled = true;
                timer.Stop();
            }

        }
        private void chbSendFile_Click(object sender, RoutedEventArgs e)
        {
            if (viewMode.IsSendFile == true)
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
                    txtID.IsEnabled = txtData.IsEnabled = false;
                }
                else
                {
                    viewMode.IsSendFile = false;
                }
            }
            else
            {
                txtID.IsEnabled = txtData.IsEnabled = true;

            }
        }

        private void CheckSendText(out List<Tuple<int, byte[]>> list)
        {
            if (device == null)
            {
                throw new Exception("请先点击“开始”按钮。");
            }
            list = new List<Tuple<int, byte[]>>();
            var id = 0;
            var buffer = new byte[0];
            try
            {
                id = Convert.ToInt32(viewMode.ID, 16);
            }
            catch
            {
                throw new Exception("帧ID不符合字符要求。");
            }
            if (!string.IsNullOrEmpty(viewMode.Data))
            {
                var chars = Regex.Replace(viewMode.Data, @"\s", ""); // 剔除空格
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
            if (device == null)
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
        private void ComboBoxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (viewMode.CanManufacturer)
            {
                case 0: // 周立功
                case 1: // 广成科技
                case 2: // 创芯科技
                case 3: // iTEKon
                    cmbChannelIndex.Visibility = Visibility.Visible;
                    cmbCANBaudRate.Visibility = Visibility.Visible;
                    cmbBoardIndex.Visibility = Visibility.Visible;
                    cmbCanType.Visibility = Visibility.Visible;
                    cmbPortName.Visibility = Visibility.Collapsed;
                    cmbRS232BaudRate.Visibility = Visibility.Collapsed;
                    break;
                case 4: // 研华CAN
                    cmbChannelIndex.Visibility = Visibility.Visible;
                    cmbCANBaudRate.Visibility = Visibility.Visible;
                    cmbBoardIndex.Visibility = Visibility.Visible;
                    cmbCanType.Visibility = Visibility.Collapsed;
                    cmbPortName.Visibility = Visibility.Collapsed;
                    cmbRS232BaudRate.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    cmbChannelIndex.Visibility = Visibility.Collapsed;
                    cmbCANBaudRate.Visibility = Visibility.Collapsed;
                    cmbBoardIndex.Visibility = Visibility.Collapsed;
                    cmbCanType.Visibility = Visibility.Collapsed;
                    cmbPortName.Visibility = Visibility.Visible;
                    cmbRS232BaudRate.Visibility = Visibility.Visible;
                    cmbRS232BaudRate.SelectedIndex = 0;
                    break;
            }

        }

        private int BytesToInt32(byte[] bytes)
        {
            int result = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                result |= bytes[i] << (bytes.Length - 1 - i) * 8;
            }
            return result;
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
