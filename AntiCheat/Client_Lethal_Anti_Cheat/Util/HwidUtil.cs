using System;
using System.Management;

namespace LethalAntiCheatLauncher.Util
{
    public static class HwidUtil
    {
        private static readonly string _hwid;

        static HwidUtil()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard")) // 메인보드 시리얼
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var serial = obj["SerialNumber"]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(serial) && serial != "0" && serial.ToLower() != "none" && serial.ToLower() != "default")
                        {
                            _hwid = serial;
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(_hwid))
                    _hwid = "UNKNOWN_HWID";
            }
            catch
            {
                _hwid = "ERROR_HWID";
            }
        }

        public static string GetHwid() => _hwid;
    }
} 