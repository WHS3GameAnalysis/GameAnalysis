using System;
using System.Threading;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.ProcessWatcher;
using Lethal_Anti_Cheat.Reflection;

namespace Lethal_Anti_Cheat.Util
{
    public static class UnifiedScanner
    {
        private static readonly int intervalMs = 5000;
        private static Thread _scannerThread;
        private static bool _isRunning = false;

        public static void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _scannerThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    try
                    {
                        PipeLogger.Log($"[ScanCycle] Starting scan at {DateTime.Now:HH:mm:ss}");

                        DebugDetector.DebugDetector.RunOnce();
                        ProcessWatcher.ProcessWatcher.RunOnce();
                        NtProcessScanner.RunOnce();
                        AppDomainModuleScanner.Scan();

                        PipeLogger.Log($"[ScanCycle] Scan finished at {DateTime.Now:HH:mm:ss}");
                    }
                    catch (Exception ex)
                    {
                        PipeLogger.Log($"[FATAL] UnifiedScanner loop error: {ex.Message}");
                    }

                    Thread.Sleep(intervalMs);
                }
            })
            {
                IsBackground = true
            };
            _scannerThread.Start();
        }

        public static void Stop()
        {
            _isRunning = false;
            PipeLogger.Log("[UnifiedScanner] Scanner thread stopped.");
        }
    }
}