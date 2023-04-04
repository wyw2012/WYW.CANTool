using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    // 研华的Can
    static class AdvApi
    {
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="fileName">Can名，例如\\\\.\\can1</param>
        /// <param name="fileAccess">访问权限,可或操作</param>
        /// <param name="shareMode">默认值0。1 读；2 写；4 删除。可或操作</param>
        /// <param name="securityAttributes">NULL</param>
        /// <param name="creationDisposition">默认值3，代表打开已存在的CAN卡</param>
        /// <param name="flagsAndAttributes">异步0x40000080，同步0x80</param>
        /// <param name="templateFile">NULL</param>
        /// <returns>设备的句柄</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(string fileName, AdvFileAccess fileAccess, UInt32 shareMode, IntPtr securityAttributes, int creationDisposition, int flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr deviceHandle);

        /// <summary>
        /// 配置设备、复位、清空数据等操作
        /// </summary>
        /// <param name="deviceHandle">设备句柄，通过CreateFile方法获取</param>
        /// <param name="controlCode">控制模式。命令、配置和状态</param>
        /// <param name="inputHandle">对应控制模式的结构体句柄</param>
        /// <param name="inputSize">该结构体的长度</param>
        /// <param name="outputHandle">返回的结构体句柄，似乎没用，可给默认值IntPtr.Zero</param>
        /// <param name="outputSize">返回结构体的长度，似乎没用，可使用默认值0</param>
        /// <param name="returnSize">该方法返回的字节长度</param>
        /// <param name="errorHandle">错误句柄，可使用IntPtr.Zero</param>
        /// <returns>0 失败；非0 成功。失败时可使用GetLastError方法获取错误信息</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(IntPtr deviceHandle, AdvControlMode controlCode, IntPtr inputHandle, int inputSize, IntPtr outputHandle, int outputSize, ref int returnSize, IntPtr errorHandle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceHandle">设备句柄</param>
        /// <param name="receiveHandle">接收结构体的指针</param>
        /// <param name="maxReceiveSize">计划接收的数据字节个数</param>
        /// <param name="realReceiveSize">已接收数据字节个数</param>
        /// <param name="errorHandle">错误句柄</param>
        /// <returns>
        ///   0 失败，失败包括：1、用户取消操作或者复位芯片；2、总线忙；3、结构体参数错误；4 异步模式下操作挂起。失败时可使用GetLastError方法获取错误信息。
        /// 非0 成功。成功包含：1、所有数据接收成功；2 由于超时导致接收数据帧数量为0
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr deviceHandle, IntPtr receiveHandle, int maxReceiveSize, out int realReceiveSize, IntPtr errorHandle);

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="deviceHandle">设备句柄</param>
        /// <param name="msgWrite">数据帧</param>
        /// <param name="frameCount">准备发送数据帧数量</param>
        /// <param name="frameCountWritten">已成功发送数据帧的数量</param>
        /// <param name="errorHandle">错误句柄</param>
        /// <returns>
        ///   0 失败，失败包括：1、用户取消操作或者复位芯片；2、总线忙；3、结构体参数错误；4 异步模式下操作挂起。失败时可使用GetLastError方法获取错误信息。
        /// 非0 成功。成功包含：1、所有数据发送成功；2 由于超时发送部分数据
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr deviceHandle, AdvMessageFrame1[] msgWrite, int frameCount, out int frameCountWritten, IntPtr errorHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr deviceHandle, AdvMessageFrame2[] msgWrite, int frameCount, out int frameCountWritten, IntPtr errorHandle);


        /// <summary>
        /// 异步模式下操作未完成，需要调用该方法直到操作完成
        /// </summary>
        /// <param name="deviceHandle"></param>
        /// <param name="errorHandle"></param>
        /// <param name="numberOfBytesTransferred">已经发送或者接收的字节个数</param>
        /// <param name="isWait">true 等待操作完成；false 结束操作，操作未完成</param>
        /// <returns>0 失败；非0 成功。失败时可使用GetLastError方法获取错误信息</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetOverlappedResult(IntPtr deviceHandle, IntPtr errorHandle, out uint numberOfBytesTransferred, bool isWait);


        /// <summary>
        /// 获取监听事件类型
        /// </summary>
        /// <param name="deviceHandle">设备句柄</param>
        /// <param name="eventType">事件类型。0x80 错误事件；0x01 接收数据事件。支持或操作</param>
        /// <returns>0 失败；非0 成功。失败时可使用GetLastError方法获取错误信息</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetCommMask(IntPtr deviceHandle, out int eventType);

        /// <summary>
        /// 设置监听事件类型，如果未设置，则调用WaitCommEvent失败
        /// </summary>
        /// <param name="deviceHandle">设备句柄</param>
        /// <param name="eventType">事件类型。0x80 错误事件；0x01 接收数据事件。支持或操作</param>
        /// <returns>0 失败；非0 成功。失败时可使用GetLastError方法获取错误信息</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetCommMask(IntPtr deviceHandle, int eventType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceHandle"></param>
        /// <param name="eventType"></param>
        /// <param name="errorHandle">错误句柄</param>
        /// <returns>
        ///   0 失败，失败包括：1、用户取消操作或者复位芯片；2 异步模式下操作挂起。失败时可使用GetLastError方法获取错误信息。
        /// 非0 成功。
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WaitCommEvent(IntPtr deviceHandle,out IntPtr eventType, out IntPtr errorHandle);

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <param name="deviceHandle">设备句柄</param>
        /// <param name="errorCode">0x01 接收队列溢出；0x02 接收超时错误；0x08 接收组帧错误；0x10 检测到中断</param>
        /// <param name="deviceStatus"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ClearCommError(IntPtr deviceHandle, out int errorCode, out AdvComStatus deviceStatus);
    }
}
