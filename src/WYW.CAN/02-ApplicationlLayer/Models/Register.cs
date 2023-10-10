using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN.Models
{
    public class Register : ObservableObject
    {
        #region 构造函数
        public Register()
        {
            
           
        }
        public Register(int address)
        {
            this.Address = address;
        }

        #endregion

        #region 属性
        private bool isChecked = true;
        private RegisterValueType valueType = RegisterValueType.UInt32;
        private RegisterWriteType writeType = RegisterWriteType.读写;
        private RegisterEndianType endianType = RegisterEndianType.大端模式;
        private int address;
        private string _value = "0";
        private string description;
        private string unit;
        private OperationType operationType = OperationType.Read;
        private string variableName;

        /// <summary>
        /// 操作类型，受限于寄存器的读写类型
        /// </summary>
        public OperationType OperationType { get => operationType; set => SetProperty(ref operationType, value); }

        /// <summary>
        /// 地址
        /// </summary>
        public int Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        /// <summary>
        /// 值，字符串形式表示，可根据
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get => unit;
            set => SetProperty(ref unit, value);
        }
        /// <summary>
        /// 是否选中，选中后的可以进行读写操作
        /// </summary>
        public bool IsChecked
        {
            get => isChecked;
            set => SetProperty(ref isChecked, value);
        }
        /// <summary>
        /// 值类型，例如UInt16、Float
        /// </summary>
        public RegisterValueType ValueType
        {
            get => valueType;
            set
            {
                SetProperty(ref valueType, value);
            }
        }

        /// <summary>
        /// 端类型，默认大端对齐
        /// </summary>
        public RegisterEndianType EndianType
        {
            get => endianType;
            set => SetProperty(ref endianType, value);
        }

        /// <summary>
        /// 支持读写类型
        /// </summary>
        public RegisterWriteType WriteType
        {
            get => writeType;
            set
            {
                SetProperty(ref writeType, value);
                switch (writeType)
                {
                    case RegisterWriteType.只读:
                        OperationType = OperationType.Read;
                        break;
                    case RegisterWriteType.只写:
                        OperationType = OperationType.Write;
                        break;
                }
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        /// <summary>
        /// 变量名
        /// </summary>
        public string VariableName { get => variableName; set => SetProperty(ref variableName, value); }

        #endregion

        #region 静态方法
        /// <summary>
        /// 验证寄存器数组地址与寄存器个数是否满足要求
        /// </summary>
        /// <param name="registers"></param>
        /// <exception cref="Exception"></exception>
        public static void ValicateAddress(IEnumerable<Register> registers)
        {
            var registersArray = registers.GroupBy(x => x.WriteType); // 按读写类型分组
            foreach (var reg in registersArray)
            {
                var repeatArray = reg.GroupBy(x => x.Address).Where(x => x.Count() > 1).Select(x => x.Key);

                if (repeatArray.Count() > 0)
                {
                    throw new Exception($"“{reg.FirstOrDefault().WriteType}”类型寄存器存在重复的地址：{string.Join(", ", repeatArray)}");
                }
            }

        }
        public static void ValicateValue(IEnumerable<Register> registers)
        {
            var items = registers.Where(x => x.WriteType != RegisterWriteType.只读);
            foreach (var register in items)
            {
                switch (register.ValueType)
                {
                    case RegisterValueType.UInt32:
                        if (!UInt32.TryParse(register.Value, out _))
                        {
                            throw new Exception($"读写类型为“{register.WriteType}”且地址为{register.Address}的值错误，该值不符合{register.ValueType}");

                        }
                        break;
                    case RegisterValueType.Float:
                        if (!float.TryParse(register.Value, out _))
                        {
                            throw new Exception($"读写类型为“{register.WriteType}”且地址为{register.Address}的值错误，该值不符合{register.ValueType}");

                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 将DataTable转换成Register[]
        /// </summary>
        /// <param name="table"> DataTable表头：寄存器类型	寄存器地址	读写类型	数值类型	寄存器个数	对齐方式	值	单位	描述</param>
        /// <param name="ignoreValue">是否忽略Value列</param>
        /// <returns></returns>
        public static Register[] GetRegisters(DataTable table, bool ignoreValue = true)
        {
            int row = table.Rows.Count;
            var registers = new Register[row];
            for (int i = 0; i < row; i++)
            {
                registers[i] = new Register();
                if (table.Rows[i][0].ToString().ToLower().StartsWith("0x"))
                {
                    registers[i].Address = Convert.ToInt32(table.Rows[i][0].ToString(), 16);
                }
                else
                {
                    registers[i].Address = Convert.ToInt32(table.Rows[i][0].ToString(), 10);
                }
                registers[i].WriteType = (RegisterWriteType)Enum.Parse(typeof(RegisterWriteType), table.Rows[i][1].ToString());
                registers[i].ValueType = (RegisterValueType)Enum.Parse(typeof(RegisterValueType), table.Rows[i][2].ToString());
          
                registers[i].EndianType = (RegisterEndianType)Enum.Parse(typeof(RegisterEndianType), table.Rows[i][3].ToString());
                if (!ignoreValue)
                {
                    registers[i].Value = table.Rows[i][4]?.ToString();
                }
                registers[i].Unit = table.Rows[i][5]?.ToString();
                registers[i].Description = table.Rows[i][6]?.ToString();
                registers[i].VariableName = table.Rows[i][7]?.ToString();

            }
            return registers;
        }

        public static DataTable ToDataTable(IEnumerable<Register> registers)
        {
            DataTable table = new DataTable();
            table.Columns.Add("寄存器地址");
            table.Columns.Add("读写类型");
            table.Columns.Add("数值类型");
            table.Columns.Add("对齐方式");
            table.Columns.Add("值");
            table.Columns.Add("单位");
            table.Columns.Add("描述");
            table.Columns.Add("变量名");
            foreach (Register reg in registers)
            {
                table.Rows.Add(
                    reg.Address,
                    reg.WriteType,
                    reg.ValueType,
                    reg.EndianType,
                    reg.Value,
                    reg.Unit,
                    reg.Description);

            }
            return table;
        }
        #endregion

    }
}
