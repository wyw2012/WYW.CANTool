using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WYW.CANTool
{
    /// <summary>
    /// 沈阳广成科技Can卡API
    /// </summary>
    static class GcgdApi
    { 
        private const string LibPath = "ECanVci.dll";
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string path);

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="deviceType">Can卡设备类型，见文档</param>
        /// <param name="deviceIndex">设备索引号。比如当只有一个PCIe-9221 时，索引号为0，这时再插入一个PCIe-9221，
        /// 那么后面插入的这个设备索引号就是1，以此类推</param>
        /// <param name="reserved">保留参数，通常为0。（特例：当设备为CANET-UDP 时，此参数表示要打开的本地端口号，建议在5000 到40000 范围内取值。当设备为CANET-TCP 时，此参数固定为0。）</param>
        /// <returns>为1 表示操作成功，0 表示操作失败。</returns>
        [DllImport(LibPath)]
        public static extern UInt32 OpenDevice(UInt32 deviceType, UInt32 deviceIndex, UInt32 reserved);

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="deviceType">设备类型号</param>
        /// <param name="deviceIndex">设备索引号。对应已经打开的设备</param>
        /// <returns>为1 表示操作成功，0 表示操作失败</returns>
        [DllImport(LibPath)]
        public static extern UInt32 CloseDevice(UInt32 deviceType, UInt32 deviceIndex);
        /// <summary>
        /// 此函数用以初始化指定的CAN 通道。有多个CAN 通道时，需要多次调用
        /// </summary>
        /// <param name="deviceType">设备类型号</param>
        /// <param name="deviceIndex">设备索引号</param>
        /// <param name="canIndex">第几路CAN。即对应卡的CAN 通道号，CAN0 为0，CAN1 为1</param>
        /// <param name="initParameter">初始化参数结构</param>
        /// <returns>为1 表示操作成功，0 表示操作失败</returns>
        [DllImport(LibPath)]
        public static extern UInt32 InitCAN(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, ref ZlgInitParameter initParameter);

        /// <summary>
        /// 此函数用以获取设备信息
        /// </summary>
        /// <param name="deviceType">设备类型号</param>
        /// <param name="deviceIndex">设备索引号</param>
        /// <param name="boardInformation">用来存储设备信息的结构体</param>
        /// <returns>为1 表示操作成功，0 表示操作失败</returns>
        [DllImport(LibPath)]
        public static extern UInt32 ReadBoardInfo(UInt32 deviceType, UInt32 deviceIndex, out ZlgBoardInformation boardInformation);
        [DllImport(LibPath)]
        public static extern UInt32 ReadErrInfo(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, out ZlgErrorInformation errorInfo);

        [DllImport(LibPath)]
        static extern UInt32 ReadCANStatus(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, out ZlgBoardStatus boardStatus);
        /// <summary>
        /// 此函数用以获取设备的相应参数（主要是CANET 的相关参数）。
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <param name="refType"></param>
        /// <param name="pData">用来存储参数有关数据缓冲区地址首指针</param>
        /// <returns></returns>
        [DllImport(LibPath)]
        static extern UInt32 GetReference(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, UInt32 refType, ref byte pData);
        /// <summary>
        /// 此函数用以设置CANET 与PCI-5010-U/PCI-5020-U/USBCAN-E-U/USBCAN-2E-U/USBCAN-4E-U/CANDTU 等设备的相应参数，主要处理不同设备的特定操作。
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <param name="refType">参数类型</param>
        /// <param name="pData">用来存储参数有关数据缓冲区地址首指针</param>
        /// <returns></returns>
        [DllImport(LibPath)]
        static extern UInt32 SetReference(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, UInt32 refType, ref byte pData);

        /// <summary>
        /// 此函数用以获取指定CAN 通道的接收缓冲区中，接收到但尚未被读取的帧数量。
        /// 主要用途是配合Receive 使用，即缓冲区有数据，再接收。用户无需一直调用Receive，可以节约PC 系统资源，提高程序效率。
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <returns>返回尚未被读取的帧数</returns>
        [DllImport(LibPath)]
        public static extern UInt32 GetReceiveNum(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex);
        /// <summary>
        /// 此函数用以清空指定CAN 通道的缓冲区。主要用于需要清除接收缓冲区数据的情况。
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex">为1 表示操作成功，0 表示操作失败</param>
        /// <returns></returns>
        [DllImport(LibPath)]
        public static extern UInt32 ClearBuffer(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex);

        /// <summary>
        /// 此函数用以启动CAN 卡的某一个CAN 通道。有多个CAN 通道时，需要多次调用。
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <returns>为1 表示操作成功，0 表示操作失败。</returns>
        [DllImport(LibPath)]
        public static extern UInt32 StartCAN(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex);
        /// <summary>
        /// 此函数用以复位CAN。主要用与StartCAN 配合使用，无需再初始化，即可恢复 CAN 卡的正常状态。比如当CAN 卡进入总线关闭状态时，可以调用这个函数
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <returns>为1 表示操作成功，0 表示操作失败。（注：在CANET-TCP 中将会断开网络，需要重新StartCAN 才能使用）</returns>
        [DllImport(LibPath)]
        public static extern UInt32 ResetCAN(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <param name="sendframe"></param>
        /// <param name="frameCount">要发送的帧结构体数组的长度（发送的帧数量）</param>
        /// <returns>返回实际发送的帧数</returns>
        [DllImport(LibPath)]
        public static extern UInt32 Transmit(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex,ref ZlgMessageFrame sendframe, UInt32 frameCount);

        /// <summary>
        /// 接收函数。此函数从指定的设备CAN 通道的接收缓冲区中读取数据。建议在调用之前，
        /// 先调用GetReceiveNum。函数获知缓冲区中有多少帧，然后对应地去接收
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <param name="receiveFrame"></param>
        /// <param name="frameCount">用来接收的帧结构体数组的长度（本次接收的最大帧数，实际返回值小于等于这个值）。</param>
        /// <param name="waitTime">缓冲区无数据，函数阻塞等待时间，以毫秒为单位。若为-1 则表示无超时，一直等待</param>
        /// <returns>返回实际读取到的帧数。如果返回值为0xFFFFFFFF，则表示读取数据失败，有错误发生，请调用ReadErrInfo 函数来获取错误码。</returns>
        [DllImport(LibPath, CharSet = CharSet.Ansi)]
        public static extern UInt32 Receive(UInt32 deviceType, UInt32 deviceIndex, UInt32 canIndex, out ZlgMessageFrame receiveFrame, UInt32 frameCount, Int32 waitTime);
    }
}
