using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CANTool.Models
{
    internal class Config:ObservableObject
    {

        public CanConfig CAN { get; } =new CanConfig();
        public DisplayConfig Display { get; } = new DisplayConfig();
        public SendConfig Send { get; } = new SendConfig();
        public StatusConfig Status { get; } = new StatusConfig();

    }
    class CanConfig : ObservableObject
    {
        private int canManufacturer = 1;
        private int canType = 4;
        private int boardIndex;
        private int channelIndex;
        private int canBaudRate = 100;
        private int serialBaudRate = 9600;


        private string portName;
        /// <summary>
        /// CAN厂商，0 周立功；1 广成科技；2 创芯科技；3 艾泰科技；4 研华；5 CAN232
        /// </summary>
        public int CanManufacturer
        {
            get => canManufacturer;
            set
            {
                SetProperty(ref canManufacturer, value);
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
        public int CanBaudRate
        {
            get => canBaudRate;
            set => SetProperty(ref canBaudRate, value);
        }


        /// <summary>
        /// 串口波特率
        /// </summary>
        public int SerialBaudRate
        {
            get => serialBaudRate;
            set => SetProperty(ref serialBaudRate, value);
        }

        /// <summary>
        /// CAN通道索引
        /// </summary>
        public int ChannelIndex
        {
            get => channelIndex;
            set => SetProperty(ref channelIndex, value);
        }
        /// <summary>
        /// CAN卡索引，适用于多CAN卡场景
        /// </summary>
        public int BoardIndex
        {
            get => boardIndex;
            set => SetProperty(ref boardIndex, value);
        }

        

        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName
        {
            get => portName;
            set => SetProperty(ref portName, value);
        }
    }

    class DisplayConfig : ObservableObject
    {

        private int maxItemCount = 200;
        private bool enableDisplay = true;
        /// <summary>
        /// 最大显示的条目数量
        /// </summary>
        public int MaxItemCount
        {
            get => maxItemCount;
            set => SetProperty(ref maxItemCount, value);
        }
        /// <summary>
        /// 启用显示
        /// </summary>
        public bool EnableDisplay
        {
            get => enableDisplay;
            set => SetProperty(ref enableDisplay, value);
        }

        public ObservableCollection<string> DisplayItems { get; } = new ObservableCollection<string>();

    }
    class SendConfig : ObservableObject
    {
        private int extendFrame;
        private int remoteFrame;
        private bool isCyclic;
        private int cyclicInterval = 1000;
        private string frameID = "1";
        private string frameData = "01 02 03 04 05 06 07 08";
        /// <summary>
        /// 是否循环
        /// </summary>
        public bool IsCyclic
        {
            get => isCyclic;
            set => SetProperty(ref isCyclic, value);
        }

        /// <summary>
        /// 循环间隔时间，单位ms
        /// </summary>
        public int CyclicInterval
        {
            get => cyclicInterval;
            set => SetProperty(ref cyclicInterval, value);
        }

        /// <summary>
        /// 十六进制文本格式的ID号，例如FA
        /// </summary>
        public string FrameID
        {
            get => frameID;
            set => SetProperty(ref frameID, value);
        }
        /// <summary>
        /// 十六进制文本格式的数据，例如01 AA
        /// </summary>
        public string FrameData
        {
            get => frameData;
            set => SetProperty(ref frameData, value);
        }
        private string filePath;

        private bool isSendFile;

        /// <summary>
        /// 
        /// </summary>
        public bool IsSendFile
        {
            get => isSendFile;
            set => SetProperty(ref isSendFile, value);
        }

        /// <summary>
        /// 发送文件时文件路径
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set => SetProperty(ref filePath, value);
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

    }
    class StatusConfig : ObservableObject
    {
        private int totalReceived;

        /// <summary>
        /// 
        /// </summary>
        public int TotalReceived
        {
            get => totalReceived;
            set => SetProperty(ref totalReceived, value);
        }

        private int totalSended;

        /// <summary>
        /// 
        /// </summary>
        public int TotalSended
        {
            get => totalSended;
            set => SetProperty(ref totalSended, value);
        }

        private double progress;

        /// <summary>
        /// 
        /// </summary>
        public double Progress
        {
            get => progress;
            set => SetProperty(ref progress, value);
        }

        private bool isSending;

        /// <summary>
        /// 正在发送
        /// </summary>
        public bool IsSending
        {
            get => isSending;
            set => SetProperty(ref isSending, value);
        }

    }
}
