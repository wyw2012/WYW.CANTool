using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WYW.CANTool
{
    class WinAPI
    {
        /// <summary>
        /// 获取定时器精度
        /// </summary>
        /// <param name="MaximumResolution">最大分辨率，单位100ns</param>
        /// <param name="MinimumResolution">最小分辨率，单位100ns</param>
        /// <param name="ActualResolution">当前分辨率，单位100ns</param>
        /// <returns></returns>
        [DllImport("NTDLL.dll", SetLastError = true)]
        public static extern int NtQueryTimerResolution(out uint MaximumResolution, out uint MinimumResolution, out uint ActualResolution);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RequestedResolution">The desired timer resolution. Must be within the legal range of system timer values supported by NT. On standard x86 systems this is 1-10 milliseconds. Values that are within the acceptable range are rounded to the next highest millisecond boundary by the standard x86 HAL. This parameter is ignored if the Set parameter is FALSE.</param>
        /// <param name="SetResolution">This is TRUE if a new timer resolution is being requested, and FALSE if the application is indicating it no longer needs a previously implemented resolution.</param>
        /// <param name="ActualResolution">The timer resolution in effect after the call is returned in this parameter.</param>
        /// <returns></returns>
        [DllImport("NTDLL.dll", SetLastError = true)]
        public static extern int NtSetTimerResolution(uint RequestedResolution, bool SetResolution, out uint ActualResolution);
    }
}
