using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using System.Collections.Generic;
using System.Reflection;

namespace LethalAntiCheat.AntiCheats
{
    public static class HarmonyPatchDetect
    {
        private static Harmony harmonyInstance;

        public static void ReceivingIllegalPatch(Harmony harmony)
        {
            harmonyInstance = harmony;
            PatchDetector.IllegalPatchFound += HandleIllegalPatch;
        }

        private static void HandleIllegalPatch(MethodBase method, IEnumerable<string> owners)
        {
            string ownerList = string.Join(", ", owners);

            if (method.DeclaringType == typeof(PlayerControllerB))
            {
                MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] suspicious patch on PlayerControllerB.{method.Name} by: {ownerList}");
                harmonyInstance.Patch(method, prefix: new HarmonyMethod(typeof(PatchInterceptor), nameof(PatchInterceptor.InterceptAndKick)));
            }
            else if (method.DeclaringType == typeof(EnemyAI))
            {
                MessageUtils.ShowHostOnlyMessage($"[!] WARNING: suspicious patch detected on EnemyAI.{method.Name} by: {ownerList}");

            }
            else
            {
                MessageUtils.ShowHostOnlyMessage($"[!] WARNING: suspicious patch detected on {method.DeclaringType?.FullName}.{method.Name} by: {ownerList}");
            }
        }

        private static class PatchInterceptor
        {
            public static bool InterceptAndKick(object __instance, MethodBase __originalMethod)
            {
                if (__instance is PlayerControllerB localPlayer && !localPlayer.IsHost)
                {
                    AntiManager.Instance.KickPlayer(localPlayer, $"Illegal patch used on: {__originalMethod.Name}");
                    return false; // 원본 메서드 및 악성 패치 실행 차단
                }
                return true; // 호스트거나 PlayerControllerB가 아니면 원래 로직대로 실행
            }
        }
    }
}
