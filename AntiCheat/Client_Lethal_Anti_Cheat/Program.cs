using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using SharpMonoInjector;

namespace LethalAntiCheatLauncher
{
    internal class Program
    {
        private const string TargetProcessName = "Lethal Company";
        private const string DllName = "Lethal_Anti_Cheat.dll";
        private const string Namespace = "Lethal_Anti_Cheat";
        private const string ClassName = "Loader";
        private const string MethodName = "Init";
        private const string PipeName = "AntiCheatPipe";

        private static object _lock = new();
        private static bool _injectionComplete = false;

        static void Main(string[] args)
        {
            Console.Title = "Lethal Anti-Cheat Client";
            Console.Clear();
            Console.WriteLine("[AntiCheat] Client started. Waiting for game process...\n");

            InjectDllWhenGameStarts();
            StartConsoleClearLoop();
            ListenForLogs();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        static void InjectDllWhenGameStarts()
        {
            while (true)
            {
                try
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
                                Console.WriteLine("[✓] Injection successful. Anti-cheat initialized.");
                                Console.ResetColor();
                            }

                            _injectionComplete = true;
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
                catch (Exception ex)
                {
                    lock (_lock)
                    {
                        Console.WriteLine($"[Error] {ex.Message}");
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        static void ListenForLogs()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        server.WaitForConnection();

                        using var reader = new StreamReader(server);
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            lock (_lock)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[DLL] {line}");
                                Console.ResetColor();
                            }
                        }
                    }
                    catch
                    {
                        Thread.Sleep(10);
                    }
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        static void StartConsoleClearLoop()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    if (_injectionComplete)
                    {
                        lock (_lock)
                        {
                            Console.Clear();
                            Console.WriteLine($"[AntiCheat] Console refreshed at {DateTime.Now:HH:mm:ss}");
                        }
                    }
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}
