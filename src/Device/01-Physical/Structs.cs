using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CANTool
{
    #region 周立功、广成科技、创芯科技、iTEKonAPI结构体
    /// <summary>
    /// ZLG CAN系列接口卡信息的数据类型。
    /// </summary>
    struct ZlgBoardInformation
    {
        public UInt16 HardwareVersion;
        public UInt16 FirmwareVersion;
        public UInt16 DriverVersion;
        public UInt16 InterfaceVersion;

        /// <summary>
        /// 板卡所使用的中断号
        /// </summary>
        public UInt16 InterruptNumber;

        /// <summary>
        /// 表示有几路CAN 通道
        /// </summary>
        public byte ChannelCount;

        /// <summary>
        /// 此板卡的序列号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] SerialNumber;
        /// <summary>
        /// 硬件类型
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] HardwareType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt16[] Reserved;


    }

    /// <summary>
    /// USB-CAN总线适配器板卡信息的数据类型，该类型为VCI_FindUsbDevice函数的返回参数
    /// </summary>
    struct ZlgBoardInformationEx
    {
        public UInt16 HardwareVersion;
        public UInt16 FirmwareVersion;
        public UInt16 DriverVersion;
        public UInt16 InterfaceVersion;

        /// <summary>
        /// 板卡所使用的中断号
        /// </summary>
        public UInt16 InterruptNumber;

        /// <summary>
        /// 表示有几路CAN 通道
        /// </summary>
        public byte ChannelCount;

        public byte Reserved;
        /// <summary>
        /// 此板卡的序列号
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] SerialNumber;
        /// <summary>
        /// 硬件类型
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] HardwareType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] UsbSerialNumber;
    }

    /// <summary>
    /// CAN信息帧的数据类型。
    /// </summary>
    struct ZlgMessageFrame
    {
        /// <summary>
        /// 帧ID,右对齐
        /// </summary>
        public UInt32 ID;
        /// <summary>
        /// 设备接收到某一帧的时间标识,时间标示从CAN 卡上电开始计时，计时单位为0.1ms
        /// </summary>
        public UInt32 TimeStamp;
        /// <summary>
        /// 是否使用时间标识。为1 时TimeStamp 有效，TimeFlag 和TimeStamp 只在此帧为接收帧时有意义。
        /// </summary>
        public byte TimeFlag;
        /// <summary>
        /// 发送帧类型。
        /// =0 时为正常发送（发送失败会自动重发，重发最长时间为1.5-3 秒）；
        /// =1 时为单次发送（只发送一次，不自动重发）；
        /// =2 时为自发自收（自测试模式，用于测试CAN 卡是否损坏）；
        /// =3 时为单次自发自收（单次自测试模式，只发送一次）。
        /// 只在此帧为发送帧时有意义。
        /// </summary>
        public byte SendType;
        /// <summary>
        /// 是否是远程帧。=0 时为为数据帧，=1 时为远程帧（数据段空）
        /// </summary>
        public byte RemoteFlag;
        /// <summary>
        /// 是否是扩展帧。=0 时为标准帧（11 位ID），=1 时为扩展帧（29 位ID）
        /// </summary>
        public byte ExternFlag;
        /// <summary>
        /// 数据长度DLC ,小于等于8，即CAN 帧Data 有几个字节。约束了后面Data[8]中的有效字节。
        /// </summary>
        public byte DataLength;
        /// <summary>
        /// CAN 帧的数据。由于CAN 规定了最大是8 个字节，所以这里预留了8 个字节的空间，
        /// 受DataLen 约束。如DataLen 定义为3，即Data[0]、Data[1]、Data[2] 是有效的
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved;

    }

    /// <summary>
    /// 初始化Can配置
    /// </summary>
    struct ZlgInitParameter
    {
        /// <summary>
        /// 验收码。SJA1000 的帧过滤验收码。对经过屏蔽码过滤为“有关位”进行匹配，
        /// 全部匹配成功后，此帧可以被接收。否则不接收
        /// </summary>
        public UInt32 AccCode;
        /// <summary>
        /// 屏蔽码。SJA1000 的帧过滤屏蔽码。对接收的CAN 帧ID 进行过滤，对应位为0 的是“有关位”，对应位为1 的是“无关位”。屏蔽码推荐设置为0xFFFFFFFF，即全部接收。
        /// </summary>
        public UInt32 AccMask;
        public UInt32 Reserved;
        /// <summary>
        /// 滤波方式。=1 表示单滤波，=0 表示双滤波,2标准帧滤波，3是扩展帧滤波。
        /// </summary>
        public byte Filter;
        /// <summary>
        /// 波特率定时器0（BTR0）。设置值见下表
        /// </summary>
        public byte Timing0;
        /// <summary>
        /// 波特率定时器1（BTR1）。设置值见下表。
        /// </summary>
        public byte Timing1;
        /// <summary>
        /// 模式。=0 表示正常模式（相当于正常节点），=1 表示只听模式（只接收，不影响总线）=2 表示自发自收模式
        /// </summary>
        public byte Mode;
    }

    struct ZlgBoardStatus
    {
        /// <summary>
        /// 中断记录，读操作会清除中断
        /// </summary>
        public byte ErrorInterrupt;
        /// <summary>
        /// CAN 控制器模式寄存器值
        /// </summary>
        public byte RegisterMode;
        /// <summary>
        /// CAN 控制器状态寄存器值
        /// </summary>
        public byte RegisterStatus;
        /// <summary>
        /// CAN 控制器仲裁丢失寄存器值
        /// </summary>
        public byte RegisterALCapture;
        /// <summary>
        /// CAN 控制器错误寄存器值
        /// </summary>
        public byte RegisterECCapture;
        /// <summary>
        /// CAN 控制器错误警告限制寄存器值。默认为96。
        /// </summary>
        public byte RegisterEWLimit;
        /// <summary>
        /// CAN 控制器接收错误寄存器值。为0-127 时，为错误主动状态，为128-254 为错误被动状态，为255 时为总线关闭状态。
        /// </summary>
        public byte RegisterRECounter;
        /// <summary>
        /// CAN 控制器发送错误寄存器值。为0-127 时，为错误主动状态，为128-254 为错误被动状态，为255 时为总线关闭状态
        /// </summary>
        public byte RegisterTECounter;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;
    }

    /// <summary>
    /// Can卡错误信息
    /// </summary>
    struct ZlgErrorInformation
    {
        public UInt32 ErrorCode;
        /// <summary>
        /// 当产生的错误中有消极错误时表示为消极错误的错误标识数据
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] PassiveErrorData;
        /// <summary>
        /// 当产生的错误中有仲裁丢失错误时表示为仲裁丢失错误的错误标识数据
        /// </summary>
        public byte ArbitraterLostErrorData;
    }


    /// <summary>
    /// 适用于周立功、广成科技、创芯科技
    /// </summary>
    public enum ZlgBoardType
    {
        PCI5121 = 1,
        PCI9810 = 2,
        USBCAN1 = 3,
        USBCAN2 = 4,
        PCI9820 = 5,
        CAN232 = 6,
        PCI5110 = 7,
        CANmini = 8,
        ISA9620 = 9,
        ISA5420 = 10,
        PC104CAN = 11,
        CANETUDP = 12,
        DNP9810 = 13,
        PCI9840 = 14,
        PC104CAN2 = 15,
        PCI9820I = 16,
        CANETTCP = 17,
        PEC9920 = 18,
        PCIE9220 = 18,
        PCI5010U = 19,
        USBCANEU = 20,
        USBCAN2EU = 21,
        PCI5020U = 22,
        EG20TCAN = 23,
        PCIE9221 = 24,
        CANWIFITCP = 25,
        CANWIFIUDP = 26,
        PCIE9120I = 27,
        PCIE9110I = 28,
        PCIE9140I = 29,
        USBCAN4EU = 31,
        CANDTU = 32,
        USBCAN8EU=34,
        CANDTUNET=36
    }

    #endregion

    #region 研华API结构体
    /// <summary>
    /// 旧版本驱动结构体
    /// </summary>
    struct AdvMessageFrame1
    {
        /// <summary>
        /// 8-bit flags are used to record types of messages during sending or receiving.The meanings of these bits are:
        /// <para>Bit Declaration  Meaning Description </para>
        /// <para>0 MSG_RTR RTR               1 means Remote frame,                           0 means data frame.</para>
        /// <para>1 MSG_OVR Hardware OVERRUN  1 means receive buffer overrun of hardware.</para>
        /// <para>2 MSG_EXT Extension         1 means Extended frame(29 bit identifier),      0 means Standard frame(11 bit identifier). </para>
        /// <para>3 MSG_SELF Self Reception   1 means self sending and receiving frame,       0 means receiving frame from other ports.</para>
        /// <para>4 MSG_PASSIVE Error         1 means bus error.</para>
        /// <para>5 MSG_BUSOFF BUSOFF         1 means busoff. </para>
        /// <para>6 Reserved Reserved 　 </para>
        /// <para>7 MSG_BOVR Software OVERRUN 1 means receive buffer overrun of software.</para>
        /// </summary>
        public int Flag;
        /// <summary>
        /// CAN object number, used in Full CAN. 默认0
        /// </summary>
        public int ChannelIndex;    
        /// <summary>
        /// 帧ID,右对齐
        /// </summary>
        public uint ID;
        /// <summary>
        /// 数据长度DLC ,小于等于8，即CAN 帧Data 有几个字节。约束了后面Data[8]中的有效字节。
        /// </summary>
        public short DataLength;                                    
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data;
    }

    /// <summary>
    /// 新版本驱动结构体
    /// </summary>
    struct AdvMessageFrame2
    {
        /// <summary>
        /// 8-bit flags are used to record types of messages during sending or receiving.The meanings of these bits are:
        /// <para>Bit Declaration  Meaning Description </para>
        /// <para>0 MSG_RTR RTR               1 means Remote frame,                           0 means data frame.</para>
        /// <para>1 MSG_OVR Hardware OVERRUN  1 means receive buffer overrun of hardware.</para>
        /// <para>2 MSG_EXT Extension         1 means Extended frame(29 bit identifier),      0 means Standard frame(11 bit identifier). </para>
        /// <para>3 MSG_SELF Self Reception   1 means self sending and receiving frame,       0 means receiving frame from other ports.</para>
        /// <para>4 MSG_PASSIVE Error         1 means bus error.</para>
        /// <para>5 MSG_BUSOFF BUSOFF         1 means busoff. </para>
        /// <para>6 Reserved Reserved 　 </para>
        /// <para>7 MSG_BOVR Software OVERRUN 1 means receive buffer overrun of software.</para>
        /// </summary>
        public int Flag;
        /// <summary>
        /// CAN object number, used in Full CAN. 默认0
        /// </summary>
        public int ChannelIndex;
        /// <summary>
        /// 帧ID,右对齐
        /// </summary>
        public uint ID;
        /// <summary>
        /// 接收消息的时间戳，驱动V1.37之后的版本才有，V1.35版本无该结构
        /// </summary>
        public uint Second;
        /// <summary>
        /// 接收消息的时间戳，驱动V1.37之后的版本才有，V1.35版本无该结构
        /// </summary>
        public uint Millisecond;
        /// <summary>
        /// 数据长度DLC ,小于等于8，即CAN 帧Data 有几个字节。约束了后面Data[8]中的有效字节。
        /// </summary>
        public short DataLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data;
    }

    struct AdvComStatus
    {
        public int fCtsHold;
        public int fDsrHold;
        public int fRlsdHold;
        public int fXoffHold;
        public int fXoffSent;
        public int fEof;
        public int fTxim;
        public int fReserved;
        public int cbInQue;
        public int cbOutQue;
    }
    enum AdvControlMode
    {
        Command = 0x222540,
        Config = 0x222544,
        Status = 0x222554,
    }
    enum AdvCommand
    {
        StartChip=1,
        StopChip=2,
        ResetChip=3,
        ClearReceiveBuffer=4,
    }
    /// <summary>
    /// 控制模式为Comand和Config时的入参
    /// </summary>
    struct AdvParameter
    {
        public int Command;                      // AdvControlMode=Command下有效
        public int Config;                       // AdvControlMode=Config下有效
        public uint Parameter1;                  // parameter 1
        public uint Parameter2;                  // parameter 2 
        public int Error;                        // return value
        public int Return;                       // return value

        //------------------控制模式为“参数”时的含义------------------------
        // Command    Config    Parameter1                             Parameter2
        //    0         0       AccMask                                AccCode
        //    0         1       AccMask (例如：0xFFFFFFFF)             
        //    0         2       AccCode (例如：0x00000000)      
        //    0         3       BaudRate (例如：100K的参数为100)   
        //    0         8       Listen Mode (0 正常;1 只听模式)        
        //    0         9       Self Accept (0 正常;1 自发自收)       
        //    0         13      Write Timeout,Unit:ms                   Read Timeout,Unit:ms
        //    0         20      Filter (0 双滤波；1 单滤波)      

        //------------------控制模式为“命令”时的含义------------------------
        //  Command   Config    Parameter1                             Parameter2    
        //    1         0       Start chip
        //    2         0       Stop chip(固定值1)                     (固定值1)  
        //    3         0       Reset chip          
        //    4         0       Clear the receive buffer          

    }
    /// <summary>
    /// 控制模式为Status时的出参
    /// </summary>
    struct AdvStatus
    {
        public uint baud;                      //Actual bit rate 
        public uint status;                    //The status register (SR); CAN address 2.
        public uint error_warning_limit;       //The error warning limit register (EWLR); CAN address 13.
        public uint rx_errors;                 //The RX error counter register (RXERR); CAN address 14.
        public uint tx_errors;                 //The TX error counter register (TXERR); CAN address 15.
        public uint error_code;                //The error code capture register (ECC); CAN address 12.
        public uint rx_buffer_size;            //Size of rx buffer
        public uint rx_buffer_used;            //number of messages
        public uint tx_buffer_size;            //Size of tx buffer for wince, windows not use tx buffer
        public uint tx_buffer_used;            //Number of message for wince, windows not use tx buffer s
        public uint retval;                    //Return Value. 0, SUCCESS; 0xFFFFFFFF, FAIL.
        public uint type;                      //CAN controller type. 1, SJA1000; 0 other device.
        public uint acceptancecode;            //Acceptance code 
        public uint acceptancemask;            //Acceptance mask
        public uint acceptancemode;            //Acceptance Filter Mode: 1:Single 0:Dual
        public uint selfreception;             //Self reception 
        public uint readtimeout;               //Read timeout 
        public uint writetimeout;              //Write timeout 
    }
  

    enum AdvFileAccess
    {
        Read = unchecked((int)0x80000000),
        Write = 0x40000000,
        Execute = 0x20000000,
        All = 0x10000000,
    }

    enum AdvTransferMode
    {
        /// <summary>
        /// 异步
        /// </summary>
        Async= 0x40000080,
        /// <summary>
        /// 同步
        /// </summary>
        Sync=0x80
    }
    #endregion
}
