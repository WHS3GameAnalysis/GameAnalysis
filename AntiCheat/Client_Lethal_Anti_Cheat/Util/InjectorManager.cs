using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SharpMonoInjector;

namespace LethalAntiCheatLauncher
{
    public static class InjectorManager
    {
        private const string TargetProcessName = "Lethal Company";
        private const string DllName = "Lethal_Anti_Cheat.dll";
        private const string Namespace = "Lethal_Anti_Cheat";
        private const string ClassName = "Loader";
        private const string MethodName = "Init";

        public static bool InjectionCompleted { get; private set; } = false;
        private static readonly object _lock = new();
        private static Injector _injector;
        private static IntPtr _assemblyHandle;

        public static void InjectWhenGameStarts()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var proc = Process.GetProcessesByName(TargetProcessName).FirstOrDefault();
                    if (proc != null)
                    {
                        lock (_lock)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"[*] Game process detected: {proc.ProcessName} (PID: {proc.Id})");
                            Console.ResetColor();
                        }

                        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
                        if (!File.Exists(dllPath))
                        {
                            lock (_lock)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[Error] DLL not found: {dllPath}");
                                Console.ResetColor();
                            }
                            return;
                        }

                        try
                        {
                            Console.WriteLine("[*] Waiting for the game to initialize... (5 seconds)");
                            Thread.Sleep(5000); // 5초 대기
                            _injector = new Injector(proc.Id);
                            _assemblyHandle = (IntPtr)_injector.Inject(
                                File.ReadAllBytes(dllPath),
                                Namespace,
                                ClassName,
                                MethodName
                            );

                            lock (_lock)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("[!] Injection successful. Anti-cheat initialized.");
                                Console.ResetColor();
                            }

                            InjectionCompleted = true;
                            break;
                        }
                        catch (InjectorException ex)
                        {
                            lock (_lock)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[Error] Injection failed: {ex.Message}");
                                Console.ResetColor();
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }

        public static void UnloadAntiCheat()
        {
            if (_injector == null || _assemblyHandle == IntPtr.Zero)
            {
                Console.WriteLine("[AntiCheat] Lethal_Anti_Cheat.dll not injected or already unloaded.");
                return;
            }

            try
            {
                // "Unload" 메서드는 Lethal_Anti_Cheat.dll의 Loader 클래스에 구현되어 있어야 합니다.
                _injector.Eject(_assemblyHandle, Namespace, ClassName, "Unload");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[AntiCheat] Lethal_Anti_Cheat.dll successfully unloaded.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Failed to unload Lethal_Anti_Cheat.dll: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}