using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using Unity.Netcode;

namespace LethalAntiCheat.AntiCheats
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
            if (victim == null || victim.isPlayerDead || damageAmount <= 0) return;

            if (oldHealth == victim.health)
            {
                MessageUtils.ShowMessage($"{victim.playerUsername} is using God Mode");
                AntiManager.Instance.KickPlayer(victim, "God Mode");
            }
        }
    }
}