using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using HarmonyLib;

namespace LethalAntiCheat.Core
{
    public static class PatchDetector
    {
        public static event Action<MethodBase, IEnumerable<string>> IllegalPatchFound;

        private static Timer timer;
        private static readonly HashSet<MethodBase> compromisedMethods = new HashSet<MethodBase>();
        private const string AntiCheatHarmonyId = "LethalAntiCheat";

        public static void Start()
        {
            MessageUtils.ShowHostOnlyMessage("[LethalAntiCheat] Patch Detector Initializing...");
            timer = new Timer(30000); // 30초마다 검사
            timer.Elapsed += (sender, e) => CheckForNewPatches();
            timer.AutoReset = true;
            timer.Enabled = true;
            CheckForNewPatches(); // 즉시 첫 검사
        }

        private static void CheckForNewPatches()
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (assembly == null) return;

                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (method.IsAbstract || method.ContainsGenericParameters || compromisedMethods.Contains(method)) continue;

                        var patchInfo = Harmony.GetPatchInfo(method);
                        if (patchInfo == null || !patchInfo.Owners.Any()) continue;

                        var unauthorizedOwners = patchInfo.Owners.Where(owner => owner != AntiCheatHarmonyId && !owner.StartsWith("harmony-auto-")).ToList();

                        if (unauthorizedOwners.Any())
                        {
                            compromisedMethods.Add(method);
                            // 의심스러운 패치가 발견되면 구독한 HarmonyPatchDetect로 패치 이름/주체를 보냄
                            IllegalPatchFound?.Invoke(method, unauthorizedOwners);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageUtils.ShowHostOnlyMessage($"[X] PatchDetector Error: {ex.Message}");
            }
        }
    }
}

