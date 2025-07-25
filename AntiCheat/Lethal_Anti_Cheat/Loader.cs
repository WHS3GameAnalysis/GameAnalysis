using System;
using Lethal_Anti_Cheat.Util;
using Lethal_Anti_Cheat.Reflection;
using Lethal_Anti_Cheat.DLLDetector;
using Lethal_Anti_Cheat.HarmonyPatchDetector;


namespace Lethal_Anti_Cheat
{
    public class Loader
    {
        public static void Init()
        {
            // ConsoleManager.Initialize(); // 클라이언트에서 콘솔을 관리하므로 주석 처리

            PipeLogger.Log("[AntiCheat] Module Init Complete.");

            DebugDetector.DebugDetector.Init();
            AppDomainModuleScanner.Initialize();
            ReflectionDetector.StartScheduledHashScan();
            SandboxAppDomain.InitializeSandbox();
            CheckDLL.Start();
            HarmonyPatchDetector.HarmonyPatchDetector.Start();

            UnifiedScanner.Start();
        }

        public static void Unload()
        {
            PipeLogger.Log("[AntiCheat] Unloading modules...");

            UnifiedScanner.Stop();
            CheckDLL.Stop();
            HarmonyPatchDetector.HarmonyPatchDetector.Stop();
            // 추가적인 정리 로직이 필요하다면 여기에 구현

            PipeLogger.Log("[AntiCheat] All modules unloaded.");
        }
    }
}