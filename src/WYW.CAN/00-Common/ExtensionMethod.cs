using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ExtensionMethod
    {
        /// <summary>
        /// 将字节数组转换成十六进制显示的字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToHexString(this IEnumerable<byte> source)
        {
            return string.Join(" ", source.Select(x => x.ToString("X2")));
        }
        /// <summary>
        /// 十六进制字符串转换成字节数组
        /// </summary>
        /// <param name="text">十六进制字符串</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] ToHexArray(this string text)
        {
            var result = new List<byte>();
            var chars = Regex.Replace(text, @"\s", ""); // 剔除空格
            if (chars.Length % 2 == 1)
            {
                throw new ArgumentException($"字符串长度需要是偶数");
            }
            var hexs = Regex.Split(chars, @"(?<=\G.{2})(?!$)");   // 两两分组
            try
            {
                result.AddRange(hexs.Select(x => Convert.ToByte(x, 16)).ToArray());
            }
            catch
            {
                throw new ArgumentException($"字符串无法转换成十六进制");
            }
            return result.ToArray();
        }
        /// <summary>
        /// 将字节数组转换成UTF-8格式的字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToASCII(this byte[] source)
        {
            return Encoding.ASCII.GetString(source);
        }
        /// <summary>
        /// 将字节数组转换成UTF-8格式的字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToUTF8(this byte[] source)
        {
            return Encoding.UTF8.GetString(source);
        }

        /// <summary>
        /// 将字节数组转换成UTF-8格式的字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startIndex">起始地址</param>
        /// <param name="length">字节长度</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ToUTF8(this byte[] source, int startIndex, int length)
        {
            if (startIndex + length > source.Length)
            {
                throw new ArgumentException("参数错误，起始索引与长度之和大于字节数组长度");
            }
            return Encoding.UTF8.GetString(source, startIndex, length);
        }
        public static byte[] SubBytes(this byte[] source, int startIndex, int length)
        {
            if (startIndex + length > source.Length)
            {
                throw new ArgumentException("参数错误，起始索引与长度之和大于字节数组长度");
            }
            var result = new byte[length];
            Array.Copy(source, startIndex, result, 0, length);
            return result;
        }
        /// <summary>
        /// 获取枚举的DescriptionAttribute标记内容
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum source)
        {
            var field = source.GetType().GetField(source.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return customAttribute == null ? source.ToString() : ((DescriptionAttribute)customAttribute).Description;
        }
        //从给定的枚举类型Type中，通过枚举描述找到对应的枚举值，
        public static Enum GetEnumByDescription<T>(this string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return null;
            }
            Array values = Enum.GetValues(typeof(T));
            foreach (Enum item in values)
            {
                if (description == GetDescription(item))
                {
                    return item;
                }
            }
            return null;
        }
        public static string GetMD5(this byte[] source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(source);
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }
        public static string GetMD5(this string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return bytes.GetMD5();
        }
        /// <summary>
        /// 匹配并返回字符串中第一个浮点数
        /// </summary>
        /// <param name="source"></param>
        /// <returns>匹配不成功则返回null</returns>
        public static double? RegexDouble(this string source)
        {
            var matchData = Regex.Match(source, @"(-?.[\d]){1,20}");
            while (matchData.Success)
            {
                return double.Parse(matchData.Value);
            }
            return null;
        }
        /// <summary>
        /// 剔除特殊字符，使之满足windows文件命名规则
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string ToFileName(this string chars)
        {
            return Regex.Replace(chars, @"[\/\\\|\<\>\*\:\?]", " ");
        }
        /// <summary>
        /// 判断字符串是否是IP地址
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static bool IsIPV4(this string ipAddress)
        {
           return Regex.IsMatch(ipAddress, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        }
        /// <summary>
        /// 每两个字符之间加入空格
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddSpace(this string text)
        {
            return string.Join(" ", Regex.Split(text, @"(?<=\G.{2})(?!$)"));
        }

        public static IEnumerable<T> DataTableToList<T>(this DataTable dataTable)
        {
            List<T> list = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T item = Activator.CreateInstance<T>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    PropertyInfo property = item.GetType().GetProperty(column.ColumnName);
                    if (property != null)
                    {
                        property.SetValue(item, row[column], null);
                    }
                }
                list.Add(item);
            }
            return list;
        }
    }
}
