using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class GodMode
    {
        private static readonly Dictionary<ulong, int> DamagedPlayerHealth = new Dictionary<ulong, int>();

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        public static void Prefix(PlayerControllerB __instance, int damageNumber)
        {
            // 검사할 필요 없는 경우
            if (damageNumber <= 0 || __instance.isPlayerDead || __instance.isInHangarShipRoom)
            {
                return;
            }

            // 데미지를 입기 직전의 체력을 기록.
            ulong steamId = __instance.playerSteamId;
            if (steamId != 0)
            {
                DamagedPlayerHealth[steamId] = __instance.health;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void Postfix(PlayerControllerB __instance)
        {
            // 필요하지 않은 경우 종료.(호스트가 작동하는 경우가 아니거나, 체력 닳은 리스트에 저장하지 않은 경우)
            ulong steamId = __instance.playerSteamId;
            if (!__instance.IsHost || !DamagedPlayerHealth.ContainsKey(steamId))
            {
                return;
            }

            int previousHealth = DamagedPlayerHealth[steamId];
            DamagedPlayerHealth.Remove(steamId); // 검사 후 목록에서 제거

            //체력이 100 초과하거나 데미지를 입었음에도 이전보다 체력이 많은 경우
            if (__instance.health > previousHealth && previousHealth < 100)
            {
                AntiManager.Instance.KickPlayer(__instance, "God Mode");
            }
        }
    }
}