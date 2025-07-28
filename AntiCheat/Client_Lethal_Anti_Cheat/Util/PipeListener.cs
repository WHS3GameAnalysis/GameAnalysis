using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher
{
    public static class PipeListener
    {
        private const string PipeName = "AntiCheatPipe";

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
                                // DLL에서 오는 로그는 모두 LogSource.DLL로 처리
                                LogManager.Log(LogSource.DLL, line, Color.Yellow);
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
