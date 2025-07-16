using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace LethalAntiCheatLauncher
{
    public static class PipeListener
    {
        private const string PipeName = "AntiCheatPipe";
        private static readonly object _lock = new();

        public static void Start()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                        using var reader = new StreamReader(server);
                        server.WaitForConnection();

                        while (server.IsConnected)
                        {
                            string line = reader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                lock (_lock)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"[DLL] {line}");
                                    Console.ResetColor();
                                }
                            }
                        }
                    }
                    catch
                    {
                        Thread.Sleep(500);
                    }
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
