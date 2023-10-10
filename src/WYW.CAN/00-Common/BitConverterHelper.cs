using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// 字节转换器辅助类，支持大端转换、小端转换
    /// </summary>
    public class BitConverterHelper
    {
        public static byte[] GetBytes(char value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(bool value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(short value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(ushort value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(int value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(uint value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(long value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(ulong value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(float value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static byte[] GetBytes(double value, EndianType endianType = EndianType.LittleEndian)
        {
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.GetBytes(value).Reverse().ToArray();
                case EndianType.LittleEndian:
                    return BitConverter.GetBytes(value);
            }
            return new byte[0];
        }
        public static short ToInt16(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 2)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToInt16(value.SubBytes(startIndex, 2).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToInt16(value.ToArray(), startIndex);
            }
            return 0;
        }

        public static ushort[] ToUInt16Array(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            int length=value.Length - startIndex;
            if (length<2 || length%2==1)
            {
                throw new ArgumentException();
            }
            ushort[] result=new ushort[length/2];
            switch (endianType)
            {
                case EndianType.BigEndian:
                    for (int i=0; i<result.Length;i++)
                    {
                        result[i]= BitConverter.ToUInt16(value.SubBytes(startIndex+i*2, 2).Reverse().ToArray(), 0);
                    }
                    break;
                case EndianType.LittleEndian:
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = BitConverter.ToUInt16(value.SubBytes(startIndex + i * 2, 2).ToArray(), 0);
                    }
                    break;
            }
            return result;
        }
        public static ushort ToUInt16(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 2)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToUInt16(value.SubBytes(startIndex, 2).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToUInt16(value.ToArray(), startIndex);
            }
            return 0;
        }        public static int ToInt32(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 4)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToInt32(value.SubBytes(startIndex, 4).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToInt32(value.ToArray(), startIndex);
            }
            return 0;
        }
        public static uint ToUInt32(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 4)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToUInt32(value.SubBytes(startIndex, 4).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToUInt32(value.ToArray(), startIndex);
            }
            return 0;
        }
        public static long ToInt64(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 8)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToInt64(value.SubBytes(startIndex, 8).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToInt64(value.ToArray(), startIndex);
            }
            return 0;
        }
        public static ulong ToUInt64(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 8)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToUInt64(value.SubBytes(startIndex, 8).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToUInt64(value.ToArray(), startIndex);
            }
            return 0;
        }
        public static float ToSingle(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 4)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToSingle(value.SubBytes(startIndex, 4).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToSingle(value.ToArray(), startIndex);
            }
            return 0;
        }
        public static double ToDouble(byte[] value, int startIndex = 0, EndianType endianType = EndianType.LittleEndian)
        {
            if (value.Length < startIndex + 8)
            {
                throw new ArgumentException();
            }
            switch (endianType)
            {
                case EndianType.BigEndian:
                    return BitConverter.ToDouble(value.SubBytes(startIndex, 4).Reverse().ToArray(), 0);
                case EndianType.LittleEndian:
                    return BitConverter.ToDouble(value.ToArray(), startIndex);
            }
            return 0;
        }

    }

    public enum EndianType
    {
        /// <summary>
        /// 小端对齐
        /// </summary>
        LittleEndian = 0,
        /// <summary>
        /// 大端对齐
        /// </summary>
        BigEndian = 1,
    }
}

