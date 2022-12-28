using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CANTool
{
    /// <summary>
    /// 珠海创芯科技
    /// </summary>
    class CxgdCan : ZlgCan
    {
        public CxgdCan(ZlgBoardType boardType, int boardIndex = 0, int boudRate = 500, int channel = 0) : base(boardType, boardIndex, boudRate, channel)
        {
        }
        protected override void SetDllDirectory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lib", "CXGD");
            Environment.CurrentDirectory = path;
        }
    }
}
