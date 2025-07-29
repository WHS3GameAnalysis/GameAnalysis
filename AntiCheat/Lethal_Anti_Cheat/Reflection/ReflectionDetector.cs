using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.Reflection
{
    public static class ReflectionDetector
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(60);
        private static Dictionary<string, string> baselineHashes = new();

        private static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public static void StartScheduledHashScan()
        {
            PipeLogger.Log("[Reflection] Initializing hash snapshot...");
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

            PipeLogger.Log($"[Reflection] Baseline hash snapshot created with {baselineHashes.Count} entries.");
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
                                    PipeLogger.Log($"[Reflection] Tampering Detected: {key}");
                                    PipeLogger.Log($"[Reflection] Current:  {currentHash}");
                                    PipeLogger.Log($"[Reflection] Baseline: {baselineHash}");
                                    PipeLogger.Log($"[Reflection] Assembly: {asm.FullName}");
                                    PipeLogger.Log($"[Reflection] PID: {Process.GetCurrentProcess().Id}\n");
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            if (issues > 0)
                PipeLogger.Log($"[Reflection] Scan Completed with {issues} issue(s).");
        }

    }
}

