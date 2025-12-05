using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PinJuke.Service
{
    public class SystemInfo
    {
        /// <summary>
        /// Returns total installed physical memory in bytes.
        /// Supports Windows (GlobalMemoryStatusEx) and Linux (/proc/meminfo).
        /// </summary>
        public int GetTotalPhysicalMemoryGigaBytes()
        {
            long bytes = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var mem = new MEMORYSTATUSEX();
                if (!GlobalMemoryStatusEx(mem))
                {
                    throw new InvalidOperationException("GlobalMemoryStatusEx failed.");
                }
                bytes = (long)mem.ullTotalPhys;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // /proc/meminfo: first line usually "MemTotal: 16392656 kB"
                var text = File.ReadAllText("/proc/meminfo");
                var m = Regex.Match(text, @"^MemTotal:\s+(\d+)\s+kB", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (m.Success && ulong.TryParse(m.Groups[1].Value, out var kb))
                {
                    bytes = (long)kb * 1024L;
                }
                else
                {
                    throw new InvalidOperationException("Unable to parse /proc/meminfo.");
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Total physical memory detection is not implemented for this OS.");
            }

            return (int)Math.Round(bytes / 1024.0 / 1024.0 / 1024.0);
        }

        #region Windows P/Invoke
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
        #endregion
    }
}
