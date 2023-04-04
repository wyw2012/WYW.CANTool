using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WYW.CANTool
{
    static class ExtensionMethod
    {
        /// <summary>
        /// 将字节输入按照十六进制格式输出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] source)
        {
            return string.Join(" ", source.Select(x => x.ToString("X2")));
        }
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
                // do nothing
            }
        }
        /// <summary>
        /// 将字节数组按照UTF-8格式输出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToUTF8(this byte[] source)
        {
            return Encoding.UTF8.GetString(source);
        }
        public static string ToASCII(this byte[] source)
        {
            return Encoding.ASCII.GetString(source);
        }
        /// <summary>
        /// 移除字符串中所有的空格、回车
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string  TrimAll(this string text)
        {
            return Regex.Replace(text, @"\s", ""); // 剔除空格
        }
        /// <summary>
        /// 根据起始索引和长度截取字节数组
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
    }
}
