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
                    try
                    {
                        PipeLogger.Log($"[ScanCycle] 시작 {DateTime.Now:HH:mm:ss}");

                        try { DebugDetector.DebugDetector.RunOnce(); }
                        catch (Exception ex) { PipeLogger.Log($"[ERROR] DebugDetector: {ex.Message}"); }

                        try { ProcessWatcher.ProcessWatcher.RunOnce(); }
                        catch (Exception ex) { PipeLogger.Log($"[ERROR] ProcessWatcher: {ex.Message}"); }

                        try { NtProcessScanner.RunOnce(); }
                        catch (Exception ex) { PipeLogger.Log($"[ERROR] NtProcessScanner: {ex.Message}"); }

                        PipeLogger.Log($"[ScanCycle] 완료 {DateTime.Now:HH:mm:ss}");
                    }
                    catch (Exception ex)
                    {
                        PipeLogger.Log($"[FATAL] UnifiedScanner 루프 오류: {ex.Message}");
                    }

                    Thread.Sleep(intervalMs);
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}
