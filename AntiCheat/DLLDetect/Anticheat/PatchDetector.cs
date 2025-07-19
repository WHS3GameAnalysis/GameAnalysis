using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using HarmonyLib;

namespace Anticheat
{
    public static class PatchDetector
    {
        private static Timer timer;

        public static void Start()
        {
            timer = new Timer(60000); // 1분마다 검사
            timer.Elapsed += CheckForPatches;
            timer.AutoReset = true;
            timer.Enabled = true;
            CheckForPatches(null, null); // 즉시 첫 검사
        }

        private static void CheckForPatches(object sender, ElapsedEventArgs e)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (assembly == null) return;

                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (method.IsAbstract || method.ContainsGenericParameters) continue;

                        var patchInfo = Harmony.GetPatchInfo(method);
                        if (patchInfo == null) continue;

                        // 이 메서드에 패치를 적용한 모든 소유자 ID를 가져옵니다.
                        var owners = patchInfo.Owners;
                        if (!owners.Any()) continue;

                        var unauthorizedOwners = owners.Where(owner => !owner.StartsWith("harmony-auto-")).ToList();

                        if (unauthorizedOwners.Any())
                        {
                            Console.WriteLine($"\n[!] Unauthorized Patch Detected: {type.FullName}.{method.Name}");
                            foreach (var owner in unauthorizedOwners)
                            {
                                Console.WriteLine($"- Detected Owner: {owner} (Not in Allowlist)");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[X] PatchDetector Error: {ex.Message}");
            }
        }
    }
}