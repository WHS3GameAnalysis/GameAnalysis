using Lethal_Anti_Cheat.Util;
using System;
using System.Diagnostics;

namespace Lethal_Anti_Cheat.ProcessWatcher
{
    public static class ProcessWatcher
    {
        private static readonly string[] TargetProcesses = {
            "cheatengine-x86_64-sse4-avx2", "x64dbg", "dnspy", "cheat engine", "ollydbg", "ida", "ghidra",
        };

        public static void RunOnce()
        {
            //Console.WriteLine("\n[ProcessWatcher] Process Scan (GetProcesses)");
            PipeLogger.Log(message: "\n[ProcessWatcher] Process Scan (GetProcesses)");

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string name = process.ProcessName.ToLower();
                    foreach (var target in TargetProcesses)
                    {
                        if (name.Contains(target))
                        {
                            //Console.WriteLine($"  - Detected Process : {name} (PID: {process.Id})");
                            PipeLogger.Log(message: $"[ProcessWatcher] - Detected Process : {name} (PID: {process.Id})");
                        }
                    }
                }
                catch
                {
                    //Console.WriteLine($"  - Error accessing process {process.ProcessName} (PID: {process.Id})");
                    PipeLogger.Log(message: $"[ProcessWatcher] [Error] accessing process {process.ProcessName} (PID: {process.Id})");
                }
            }
        }
    }
}
