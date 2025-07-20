using System;
using System.Runtime.InteropServices;
using System.IO;

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
                Console.WriteLine("[SimpleAC] DLL이 이미 로드되어 있습니다.");
                return true;
            }

            try
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string dllPath = Path.Combine(exeDirectory, DLL_NAME);

                if (!File.Exists(dllPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[SimpleAC] DLL을 찾을 수 없습니다: {dllPath}");
                    Console.ResetColor();
                    return false;
                }

                dllHandle = LoadLibrary(dllPath);

                if (dllHandle == IntPtr.Zero)
                {
                    uint error = GetLastError();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[SimpleAC] DLL 로드 실패. 오류 코드: {error}");
                    Console.ResetColor();
                    return false;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[SimpleAC] DLL이 성공적으로 로드되었습니다.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SimpleAC] DLL 로드 중 예외 발생: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static bool UnloadSimpleAC()
        {
            if (dllHandle == IntPtr.Zero)
            {
                Console.WriteLine("[SimpleAC] 언로드할 DLL이 없습니다.");
                return true;
            }

            try
            {
                bool result = FreeLibrary(dllHandle);
                if (result)
                {
                    dllHandle = IntPtr.Zero;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[SimpleAC] DLL이 성공적으로 언로드되었습니다.");
                    Console.ResetColor();
                }
                else
                {
                    uint error = GetLastError();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[SimpleAC] DLL 언로드 실패. 오류 코드: {error}");
                    Console.ResetColor();
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[SimpleAC] DLL 언로드 중 예외 발생: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        public static bool IsLoaded()
        {
            return dllHandle != IntPtr.Zero;
        }
    }
} 