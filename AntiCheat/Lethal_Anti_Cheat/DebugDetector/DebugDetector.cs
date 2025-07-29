using Lethal_Anti_Cheat.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lethal_Anti_Cheat.DebugDetector
{
    public static class DebugDetector
    {
        private static List<IDebugCheck> _checks;

        public static void Init()
        {
            _checks = new List<IDebugCheck>
            {
                new MonoPortScanCheck(), // mono debugger port scan check
                new RemoteDebuggerCheck(), // check for remote debugger presence
                new MonoDebuggerAttachCheck() // check if Mono debugger is attached
            };
        }

        public static void RunOnce()
        {
            var current = Process.GetCurrentProcess();
            //Console.WriteLine("\n[DebugDetector] Debugging Check");
            PipeLogger.Log(message: "\n[DebugDetector] Debugging Check");

            foreach (var check in _checks)
            {
                try
                {
                    bool debugged = check.IsDebugged(current);
                    //Console.WriteLine($"  - {check.MethodName}: Debugged? {debugged}");
                    PipeLogger.Log(message: $"[DebugDetector] - {check.MethodName}: Debugged? {debugged}");
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"  - [Error] {check.MethodName}: {ex.Message}");
                    PipeLogger.Log(message: $"[DebugDetector] [Error] {check.MethodName}: {ex.Message}");
                }
            }
        }
    }
}
