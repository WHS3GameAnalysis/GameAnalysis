using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace ReflectionTest
{
    public static class Detector
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(10);
        private static Dictionary<string, string> baselineHashes = new();

        private static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public static void StartScheduledHashScan()
        {
            Console.WriteLine("[AntiCheat] Initializing hash snapshot...");
            GenerateBaselineHashes(); 
            baselineHashes.Clear();
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(ScanInterval);
                    ScanForHashTampering();
                }
            }).Start();
        }
        private static void GenerateBaselineHashes()
        {
            baselineHashes.Clear();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.FullName.Contains("Assembly-CSharp"))
                    continue;

                foreach (var t in asm.GetTypes())
                {
                    foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        try
                        {
                            var il = m.GetMethodBody()?.GetILAsByteArray();
                            if (il != null)
                            {
                                using var sha = SHA256.Create();
                                var hash = sha.ComputeHash(il);
                                string key = t.FullName + "." + m.Name;
                                baselineHashes[key] = BytesToHex(hash);
                            }
                        }
                        catch { }
                    }
                }
            }

            Console.WriteLine($"[AntiCheat] Baseline hash snapshot created with {baselineHashes.Count} entries.");
        }

        private static void ScanForHashTampering()
        {
            int issues = 0;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.FullName.Contains("Assembly-CSharp"))
                    continue;

                foreach (var t in asm.GetTypes())
                {
                    foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        try
                        {
                            var il = m.GetMethodBody()?.GetILAsByteArray();
                            if (il != null)
                            {
                                using var sha = SHA256.Create();
                                var hash = sha.ComputeHash(il);
                                string key = t.FullName + "." + m.Name;
                                string currentHash = BytesToHex(hash);

                                if (baselineHashes.TryGetValue(key, out string baselineHash) && baselineHash != currentHash)
                                {
                                    issues++;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"[ALERT] Tampering Detected: {key}");
                                    Console.WriteLine($" Current:  {currentHash}");
                                    Console.WriteLine($" Baseline: {baselineHash}");
                                    Console.WriteLine($" Assembly: {asm.FullName}");
                                    Console.WriteLine($" PID: {Process.GetCurrentProcess().Id}\n");
                                    Console.ResetColor();
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            if (issues > 0)
                Console.WriteLine($"[Scan] Completed with {issues} issue(s).");
        }

    }
}
