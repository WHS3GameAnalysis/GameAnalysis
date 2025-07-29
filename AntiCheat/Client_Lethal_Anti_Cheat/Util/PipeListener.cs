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
                                if (line.Contains("[Behaviour]"))
                                {
                                    LogManager.Log(LogSource.Behavior, line, Color.Plum);
                                }
                                else if (line.Contains("[DebugDetector]"))
                                {
                                    LogManager.Log(LogSource.Debug, line, Color.LightSkyBlue);
                                }
                                else if (line.Contains("[HarmonyPatchDetector]"))
                                {
                                    LogManager.Log(LogSource.Harmony, line, Color.MediumPurple);
                                }
                                else if (line.Contains("[ProcessWatcher]") || line.Contains("[NtProcessScanner]"))
                                {
                                    LogManager.Log(LogSource.Process, line, Color.Orange);
                                }
                                else if (line.Contains("[Reflection]"))
                                {
                                    LogManager.Log(LogSource.Reflection, line, Color.LightCoral);
                                }
                                else if (line.Contains("[DLLDetector]"))
                                {
                                    LogManager.Log(LogSource.DLL, line, Color.Yellow);
                                }
                                else
                                {
                                    LogManager.Log(LogSource.AntiCheat, line, Color.Cyan);
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
