using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher.Util
{
    public static class SimpleACManager
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hLibModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetLastError();

        private static IntPtr dllHandle = IntPtr.Zero;
        private const string DLL_NAME = "SimpleAC.dll";

        public static bool LoadSimpleAC()
        {
            if (dllHandle != IntPtr.Zero)
            {
                LogManager.Log(LogSource.SimpleAC, "DLL is already loaded.", Color.Yellow);
                return true;
            }

            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string dllPath = Path.Combine(exeDirectory, DLL_NAME);

                if (!File.Exists(dllPath))
                {
                    LogManager.Log(LogSource.SimpleAC, $"DLL not found: {dllPath}", Color.Red);
                    return false;
                }

                dllHandle = LoadLibrary(dllPath);

                if (dllHandle == IntPtr.Zero)
                {
                    uint error = GetLastError();
                    LogManager.Log(LogSource.SimpleAC, $"DLL load failed. Error code: {error}", Color.Red);
                    return false;
                }

                LogManager.Log(LogSource.SimpleAC, "DLL loaded successfully.", Color.Green);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.SimpleAC, $"Exception while loading DLL: {ex.Message}", Color.Red);
                return false;
            }
        }

        public static bool UnloadSimpleAC()
        {
            if (dllHandle == IntPtr.Zero)
            {
                LogManager.Log(LogSource.SimpleAC, "No DLL to unload.", Color.Yellow);
                return true;
            }

            try
            {
                bool result = FreeLibrary(dllHandle);
                if (result)
                {
                    dllHandle = IntPtr.Zero;
                    LogManager.Log(LogSource.SimpleAC, "DLL unloaded successfully.", Color.Green);
                }
                else
                {
                    uint error = GetLastError();
                    LogManager.Log(LogSource.SimpleAC, $"DLL unload failed. Error code: {error}", Color.Red);
                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.SimpleAC, $"Exception while unloading DLL: {ex.Message}", Color.Red);
                return false;
            }
        }

        public static bool IsLoaded()
        {
            return dllHandle != IntPtr.Zero;
        }
    }
}
