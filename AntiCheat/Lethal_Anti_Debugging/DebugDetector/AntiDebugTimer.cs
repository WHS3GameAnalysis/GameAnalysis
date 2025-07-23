using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Lethal_Anti_Debugging
{
    public static class AntiDebugTimer
    {
        private static Stopwatch stopwatch = new Stopwatch();

        // 기준 시간 (ms)
        private static int thresholdMs = 200;

        public static void StartCheckpoint()
        {
            stopwatch.Restart();
        }

        public static void ValidateCheckpoint(string pointName = "Unknown")
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > thresholdMs)
            {
                MessageBox.Show(
                    $"Potential debugging detected at {pointName}.\nElapsed: {stopwatch.ElapsedMilliseconds} ms",
                    "Anti-Debug Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // 콘솔 로그 남기기
                Console.WriteLine($"[AntiDebug] Delay detected at {pointName}: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
