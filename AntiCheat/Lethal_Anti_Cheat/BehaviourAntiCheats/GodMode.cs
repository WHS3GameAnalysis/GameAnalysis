
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Anti_Cheat.Core;
using Unity.Netcode;
using Lethal_Anti_Cheat.Util;
using System;

namespace Lethal_Anti_Cheat.AntiCheats
{
    public static class GodMode
    {
        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayerFromOtherClientServerRpc")]
        public static class DamagePlayerFromOtherClientPatch
        {
            [HarmonyPrefix]
            public static void Prefix(PlayerControllerB __instance, int damageAmount, out int __state)
            {
                __state = __instance.health;
            }

            [HarmonyPostfix]
            public static void Postfix(PlayerControllerB __instance, int damageAmount, int __state)
            {
                CheckForGodMode(__instance, __state, damageAmount);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayerServerRpc")]
        public static class DamagePlayerServerPatch
        {
            [HarmonyPrefix]
            public static void Prefix(PlayerControllerB __instance, int damage, out int __state)
            {
                __state = __instance.health;
            }

            [HarmonyPostfix]
            public static void Postfix(PlayerControllerB __instance, int damage, int __state)
            {
                CheckForGodMode(__instance, __state, damage);
            }
        }

        private static void CheckForGodMode(PlayerControllerB victim, int oldHealth, int damageAmount)
        {
            Console.WriteLine($"[Behaviour] Checking for God Mode: {victim.playerUsername} | Old Health: {oldHealth} | Damage Amount: {damageAmount}");
            if (victim == null || victim.isPlayerDead || damageAmount <= 0) return;

            if (oldHealth == victim.health)
            {
                PipeLogger.Log($"[Behaviour] {victim.playerUsername} is using God Mode");
                AntiManager.KickPlayer(victim, "God Mode");
            }
        }
    }
}
