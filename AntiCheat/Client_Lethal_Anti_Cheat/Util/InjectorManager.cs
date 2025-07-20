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
                            var injector = new Injector(proc.Id);
                            injector.Inject(
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
    }
}
