using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CAN
{
    /// <summary>
    /// 珠海创芯科技，动态库方法命名与周立功一致
    /// </summary>
    public class CxgdCan : ZlgCan
    {
        public CxgdCan(ZlgBoardType boardType, int boardIndex = 0, int boudRate = 500, int channel = 0) : base(boardType, boardIndex, boudRate, channel)
        {
        }
        protected override void SetDllDirectory()
        {
            var bits = IntPtr.Size == 8 ? "x64" : "x86";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lib", "CXGD", bits);
            Environment.CurrentDirectory = path;
        }
    }
}
