using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch]
    public class TeleportCheck
    {
        private static Dictionary<ulong, int> teleportCounts = new Dictionary<ulong, int>();

        private const float MAX_TELEPORT_DISTANCE = 10f; // 텔레포트로 간주할 거리
        private const int MAX_TELEPORTS = 1; // 1번 감지 시 바로 강퇴

        // PlayerControllerB의 TeleportPlayer 메서드 감지
        [HarmonyPatch(typeof(PlayerControllerB), "TeleportPlayer")]
        [HarmonyPrefix]
        public static void OnTeleportAttempt(PlayerControllerB __instance, Vector3 pos)
        {
            // 조건 추가 ---------------------------------------------------------------------------------------------
            // 로컬 플레이어, 죽은 플레이어, 텔레포트 중인 플레이어는 체크 안함
            if (__instance.IsOwner || __instance.isPlayerDead || !__instance.isPlayerControlled) return;

            // 정상적인 텔레포트 상황 제외 (배 진입/퇴출, 시설 진입 등)
            if (__instance.isInElevator || __instance.isInHangarShipRoom || __instance.isInsideFactory) return;



            // 클라이언트가 아닌 호스트에서만 실행되도록 제한
            if (!NetworkManager.Singleton.IsHost)
                return;

            ulong clientId = __instance.playerClientId;
            Vector3 currentPos = __instance.transform.position;

            // 거리 측정
            float teleportDistance = Vector3.Distance(currentPos, pos);
            //Debug.Log($"[TeleportCheck] {__instance.playerUsername} teleport: {teleportDistance:F1}m");
            //Core.MessageUtils.ShowMessage($"{__instance.playerUsername} teleport: {teleportDistance:F1}m");


            // 거리 기준 초과 시 감지
            if (teleportDistance > MAX_TELEPORT_DISTANCE && !__instance.isInHangarShipRoom)
            {
                if (!teleportCounts.ContainsKey(clientId))
                    teleportCounts[clientId] = 0;

                teleportCounts[clientId]++;

                //Core.MessageUtils.ShowMessage($"[AntiCheat] {__instance.playerUsername} teleport! distance: {teleportDistance:F1}m (count: {teleportCounts[clientId]}/{MAX_TELEPORTS})");

                if (teleportCounts[clientId] >= MAX_TELEPORTS)
                {
                    AntiManager.Instance.KickPlayer(__instance, $"Teleport Hack Detected - {teleportDistance:F1}m teleport");
                    teleportCounts.Remove(clientId);
                }
            }
        }
    }
}