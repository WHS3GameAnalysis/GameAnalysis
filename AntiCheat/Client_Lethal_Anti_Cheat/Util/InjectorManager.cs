using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SharpMonoInjector;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

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
                        LogManager.Log(LogSource.AntiCheat, $"Game process detected: {proc.ProcessName} (PID: {proc.Id})", Color.Cyan);

                        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
                        if (!File.Exists(dllPath))
                        {
                            LogManager.Log(LogSource.AntiCheat, $"[Error] DLL not found: {dllPath}", Color.Red);
                            return;
                        }

                        try
                        {
                            LogManager.Log(LogSource.AntiCheat, "Waiting for the game to initialize... (5 seconds)", Color.Gray);
                            Thread.Sleep(5000);
                            _injector = new Injector(proc.Id);
                            _assemblyHandle = (IntPtr)_injector.Inject(
                                File.ReadAllBytes(dllPath),
                                Namespace,
                                ClassName,
                                MethodName
                            );

                            LogManager.Log(LogSource.AntiCheat, "Injection successful. Anti-cheat initialized.", Color.Green);

                            InjectionCompleted = true;
                            break;
                        }
                        catch (InjectorException ex)
                        {
                            LogManager.Log(LogSource.AntiCheat, $"[Error] Injection failed: {ex.Message}", Color.Red);
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
                LogManager.Log(LogSource.AntiCheat, "Lethal_Anti_Cheat.dll not injected or already unloaded.", Color.Yellow);
                return;
            }

            try
            {
                _injector.Eject(_assemblyHandle, Namespace, ClassName, "Unload");
                LogManager.Log(LogSource.AntiCheat, "Lethal_Anti_Cheat.dll successfully unloaded.", Color.Green);
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.AntiCheat, $"[Error] Failed to unload Lethal_Anti_Cheat.dll: {ex.Message}", Color.Red);
            }
        }
    }
}