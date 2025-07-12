using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Lethal_Anti_Debugging.ProcessWatcher
{
    public class ProcessWatcher
    {
        private static readonly string[] TargetProcesses = { "cheatengine-x86_64-sse4-avx2", "x64dbg", "dnspy", "cheat engine 7.5", "cheat engine" };
        private static System.Timers.Timer _timer;

        public static void StartMonitoring()
        {
            _timer = new System.Timers.Timer(3000); // 3초마다 감시
            _timer.Elapsed += CheckProcesses;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private static void CheckProcesses(object sender, ElapsedEventArgs e)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string name = process.ProcessName.ToLower();
                    if (TargetProcesses.Contains(name))
                    {
                        Console.WriteLine($"[경고] 감지된 프로세스: {name} (PID: {process.Id})");
                        // 필요 시 process.Kill(); 가능
                    }
                }
                catch { }
            }
        }
    }

}
