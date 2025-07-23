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

            if (method.DeclaringType == typeof(PlayerControllerB)) //InfinityStamina, HPDisplay, GodMode, miniMap... 플레이어 개인 핵 전체와 DamageHack(이렇게 잡는 것이 더 효율적)
            {
                MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] suspicious patch on PlayerControllerB.{method.Name} by: {ownerList}");
                harmonyInstance.Patch(method, prefix: new HarmonyMethod(typeof(HarmonyPatchDetect), nameof(HarmonyPatchDetect.InterceptAndKick)));
            }
            else if (method.DeclaringType == typeof(EnemyAI)) //ESP, EnemyList
            {
                MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] suspicious patch on EnemyAI.{method.Name} by: {ownerList}");
                //harmonyInstance.Patch(method, prefix: new HarmonyMethod(typeof(HarmonyPatchDetect), nameof(HarmonyPatchDetect.InterceptAndKick)));
            }
            else if (method.DeclaringType == typeof(EnemyType)) //EnemySpawn
            {
                MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] suspicious patch on EnemyType.{method.Name} by: {ownerList}");
                //harmonyInstance.Patch(method, prefix: new HarmonyMethod(typeof(HarmonyPatchDetect), nameof(HarmonyPatchDetect.InterceptAndKick)));
            }
            else if (method.DeclaringType == typeof(StartOfRound)) //InputSeed
            {
                MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] suspicious patch on StartOfRound.{method.Name} by: {ownerList}");
                //harmonyInstance.Patch(method, prefix: new HarmonyMethod(typeof(HarmonyPatchDetect), nameof(HarmonyPatchDetect.InterceptAndKick)));
            }
            else //알 수 없는 경우
            {
                MessageUtils.ShowHostOnlyMessage($"[!] WARNING: suspicious patch detected on {method.DeclaringType?.FullName}.{method.Name} by: {ownerList}");
            }
        }

        //패치의 내용을 탈취, 누군지 찾아 킥한다.
        
            public static bool InterceptAndKick(object __instance, MethodBase __originalMethod)
            {
                if (__instance is PlayerControllerB localPlayer && !localPlayer.IsHost)
                {
                    AntiManager.Instance.KickPlayer(localPlayer, $"Illegal patch used on: {__originalMethod.Name}");
                    return false;
                }
                return true;
            }
        
    }
}
