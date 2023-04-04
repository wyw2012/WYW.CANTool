using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WYW.CAN
{
    static class ExtensionMethod
    {
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
        public static string ToUTF8(this byte[] source)
        {
            return Encoding.UTF8.GetString(source);
        }
        public static string ToASCII(this byte[] source)
        {
            return Encoding.ASCII.GetString(source);
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
        /// 每两个字符之间加入空格
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddTrim(this string text)
        {
            return string.Join(" ", Regex.Split(text, @"(?<=\G.{2})(?!$)"));
        }
    }
}
