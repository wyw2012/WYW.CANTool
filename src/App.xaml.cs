using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WYW.CANTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
           
            uint minResolution, maxResolution, currentResolution;
            WinAPI.NtQueryTimerResolution(out maxResolution, out minResolution, out currentResolution);
            if (currentResolution > 10000)//如果定时器最小分辨率大于1ms
            {
                var result = WinAPI.NtSetTimerResolution(5000, true, out currentResolution); // 设置精度为0.5ms
                if(result!=0)
                {
                    Debug.WriteLine($"设置时间精度失败，当前时间精度为：{currentResolution / 10000.0}ms");
                }
            }
            base.OnStartup(e);
        }
    }
}
