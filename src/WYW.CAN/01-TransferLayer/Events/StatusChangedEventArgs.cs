using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// CAN状态变化或者报错信息
    /// </summary>
  public class StatusChangedEventArgs : EventArgs
    {
        public StatusChangedEventArgs(string message)
        {
            CreateTime = DateTime.Now;
            Message = message;
        }
        public DateTime CreateTime { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"[{CreateTime:HH:mm:ss.fff}] [MSG] {Message}";
        }
    }
}
