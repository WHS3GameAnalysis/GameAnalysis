using System;
using System.Threading;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.ProcessWatcher;

namespace Lethal_Anti_Cheat.Util
{
    public static class UnifiedScanner
    {
        private static readonly int intervalMs = 5000;

        public static void Start()
        {
            new Thread(() =>
            {
                while (true)
                {
                    //Console.Clear();
                    //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Lethal Anti-Cheat Scan Cycle");
                    PipeLogger.Log(message: $"\n[{DateTime.Now:HH:mm:ss}] Lethal Anti-Cheat Scan Cycle");

                    DebugDetector.DebugDetector.RunOnce();
                    ProcessWatcher.ProcessWatcher.RunOnce();
                    NtProcessScanner.RunOnce();

                    Thread.Sleep(intervalMs);
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}
