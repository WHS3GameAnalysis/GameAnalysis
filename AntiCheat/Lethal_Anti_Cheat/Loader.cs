using System;
using Lethal_Anti_Cheat.Util;
using Lethal_Anti_Cheat.Reflection;
using Lethal_Anti_Cheat.DLLDetector;
using Lethal_Anti_Cheat.HarmonyPatchDetector;
using Lethal_Anti_Cheat.AntiCheats;
using Lethal_Anti_Cheat.Core;
using HarmonyLib;
using System.Reflection;

namespace Lethal_Anti_Cheat
{
    public class Loader
    {
        public static Harmony harmony;

        public static void Init()
        {
            //ConsoleManager.Initialize();
            PipeLogger.Log("[AntiCheat] Module Init Complete.");

            harmony = new Harmony("Lethal_Anti_Cheat");

            // Original Anti-Cheat Modules
            DebugDetector.DebugDetector.Init();
            AppDomainModuleScanner.Initialize();
            ReflectionDetector.StartScheduledHashScan();
            SandboxAppDomain.InitializeSandbox();
            CheckDLL.Start();
            HarmonyPatchDetector.HarmonyPatchDetector.Start();

            // Merged Anti-Cheat Modules
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsDefined(typeof(HarmonyPatch), false))
                {
                    try
                    {
                        new PatchClassProcessor(harmony, type).Patch();
                        //Console.WriteLine($"[Behaviour] Patching {type.Name} completed.");
                        PipeLogger.Log($"[Behaviour] Patching {type.Name} completed.");
                    }
                    catch
                    {
                        // ignore
                        PipeLogger.Log($"[Behaviour] Patching {type.Name} failed.");
                    }
                }
            }

            UnifiedScanner.Start();
        }

        public static void Unload()
        {
            PipeLogger.Log("[AntiCheat] Unloading modules...");

            UnifiedScanner.Stop();
            CheckDLL.Stop();
            HarmonyPatchDetector.HarmonyPatchDetector.Stop();

            PipeLogger.Log("[AntiCheat] All modules unloaded.");
        }
    }
}