using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{

    public enum RegisterValueType
    {
        UInt32,
        Float,
    }
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperationType
    {
        Read = 0x03,
        Write =0x06,
 
    }

    public enum RegisterEndianType
    {
        /// <summary>
        /// 高位在后，例如：1 2 3 4
        /// </summary>
        小端模式 = 0,
        /// <summary>
        /// 高位在前，例如：4 3 2 1
        /// </summary>
        大端模式 = 1,
    }

    public enum RegisterWriteType
    {
        读写,
        只读,
        只写,
    }
}
