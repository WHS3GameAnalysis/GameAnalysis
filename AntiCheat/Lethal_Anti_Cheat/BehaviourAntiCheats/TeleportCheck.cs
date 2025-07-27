
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.AntiCheats
{
    [HarmonyPatch]
    public class TeleportCheck
    {
        private static Dictionary<ulong, int> teleportCounts = new Dictionary<ulong, int>();

        private const float MAX_TELEPORT_DISTANCE = 10f;
        private const int MAX_TELEPORTS = 1;

        [HarmonyPatch(typeof(PlayerControllerB), "TeleportPlayer")]
        [HarmonyPrefix]
        public static void OnTeleportAttempt(PlayerControllerB __instance, Vector3 pos)
        {
            if (__instance.IsOwner || __instance.isPlayerDead || !__instance.isPlayerControlled) return;

            if (__instance.isInElevator || __instance.isInHangarShipRoom || __instance.isInsideFactory) return;

            if (!NetworkManager.Singleton.IsHost)
                return;

            ulong clientId = __instance.playerClientId;
            Vector3 currentPos = __instance.transform.position;

            float teleportDistance = Vector3.Distance(currentPos, pos);

            if (teleportDistance > MAX_TELEPORT_DISTANCE && !__instance.isInHangarShipRoom)
            {
                if (!teleportCounts.ContainsKey(clientId))
                    teleportCounts[clientId] = 0;

                teleportCounts[clientId]++;

                if (teleportCounts[clientId] >= MAX_TELEPORTS)
                {
                    AntiManager.KickPlayer(__instance, $"Teleport Hack Detected - {teleportDistance:F1}m teleport");
                    teleportCounts.Remove(clientId);
                }
            }
        }
    }
}
