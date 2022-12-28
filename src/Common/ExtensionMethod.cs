using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CANTool
{
    static class ExtensionMethod
    {
        public static string ToHexString(this byte[] source)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < source.Length; i++)
            {
                sb.Append(source[i].ToString("X2") + " ");
            }
            return sb.ToString();
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
                throw new ArgumentException("The index was outside the bounds of the array");
            }
            var result = new byte[length];
            Array.Copy(source, startIndex, result, 0, length);
            return result;
        }
    }
}
