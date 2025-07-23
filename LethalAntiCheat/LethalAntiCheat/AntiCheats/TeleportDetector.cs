using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using HarmonyLib;

namespace LethalAntiCheat.AntiCheats
{
    public class TeleportDetector
    {
        private readonly Dictionary<ulong, PlayerTeleportData> playerData = new Dictionary<ulong, PlayerTeleportData>();
        private readonly float maxTeleportDistance = 15f; // 순간이동 감지 거리
        private readonly float minCheckInterval = 0.1f; // 최소 체크 간격
        private readonly int maxEmoteShakeCount = 5; // 정상적인 이모트 흔들림 횟수

        public void CheckPlayer(PlayerControllerB player)
        {
            if (player == null || !player.isPlayerControlled || player.isPlayerDead) return;
            if (!NetworkManager.Singleton.IsHost) return;
            if (player == GameNetworkManager.Instance.localPlayerController) return;

            ulong playerId = player.playerClientId;
            Vector3 currentPos = player.transform.position;
            float currentTime = Time.time;

            // 헬멧 위치 가져오기 (플레이어의 실제 렌더링 위치)
            Vector3 helmetPos = GetPlayerHelmetPosition(player);

            if (!playerData.ContainsKey(playerId))
            {
                playerData[playerId] = new PlayerTeleportData
                {
                    LastPosition = currentPos,
                    LastHelmetPosition = helmetPos,
                    LastCheckTime = currentTime,
                    EmoteShakeCount = 0,
                    SuspicionLevel = 0
                };
                return;
            }

            var data = playerData[playerId];
            float deltaTime = currentTime - data.LastCheckTime;

            // 너무 짧은 간격은 무시
            if (deltaTime < minCheckInterval) return;

            // 거리 계산
            float distance = Vector3.Distance(currentPos, data.LastPosition);
            float helmetDistance = Vector3.Distance(helmetPos, data.LastHelmetPosition);

            // 플레이어 상태 체크
            bool isInNormalState = CheckPlayerNormalState(player);

            // 텔레포트 감지 로직
            if (DetectTeleport(player, distance, helmetDistance, deltaTime, isInNormalState, ref data))
            {
                HandleTeleportDetection(player, distance, deltaTime);
            }

            // 데이터 업데이트
            data.LastPosition = currentPos;
            data.LastHelmetPosition = helmetPos;
            data.LastCheckTime = currentTime;
            playerData[playerId] = data;
        }

        private bool DetectTeleport(PlayerControllerB player, float distance, float helmetDistance,
            float deltaTime, bool isInNormalState, ref PlayerTeleportData data)
        {
            // 1. 순간이동 감지 (거리가 비정상적으로 큼)
            if (distance > maxTeleportDistance && isInNormalState)
            {
                data.SuspicionLevel += 3;
                return true;
            }

            // 2. 헬멧과 플레이어 위치 불일치
            float positionDiscrepancy = Math.Abs(distance - helmetDistance);
            if (positionDiscrepancy > 5f && isInNormalState)
            {
                data.SuspicionLevel += 2;
                return true;
            }

            // 3. 이동 중 이모트 흔들림이 비정상적
            if (player.performingEmote && distance > 2f)
            {
                data.EmoteShakeCount++;
                if (data.EmoteShakeCount > maxEmoteShakeCount)
                {
                    data.SuspicionLevel += 1;
                    return true;
                }
            }
            else
            {
                data.EmoteShakeCount = 0;
            }

            // 4. 속도 기반 검사 (백업)
            float speed = distance / deltaTime;
            if (speed > 25f && isInNormalState) // 매우 빠른 속도
            {
                data.SuspicionLevel += 2;
                return true;
            }

            // 의심 레벨 감소 (정상 행동시)
            if (data.SuspicionLevel > 0 && distance < 5f)
            {
                data.SuspicionLevel--;
            }

            return false;
        }

        private bool CheckPlayerNormalState(PlayerControllerB player)
        {
            // 특수 상태가 아닌 경우 (정상 이동 상태)
            return !player.inSpecialInteractAnimation &&
                   //!player.isjumping &&
                   !player.isClimbingLadder &&
                   !player.isGrabbingObjectAnimation &&
                   !player.inTerminalMenu &&
                   !player.isTypingChat &&
                   !player.isPlayerDead &&
                   !player.isInsideFactory; // 시설 입장시 순간이동 가능
        }

        private Vector3 GetPlayerHelmetPosition(PlayerControllerB player)
        {
            // 플레이어의 헬멧/카메라 위치 가져오기
            if (player.gameplayCamera != null)
            {
                return player.gameplayCamera.transform.position;
            }
            // 헬멧 위치를 못 찾으면 플레이어 위치 + 높이 보정
            return player.transform.position + Vector3.up * 1.8f;
        }

        private void HandleTeleportDetection(PlayerControllerB player, float distance, float deltaTime)
        {
            if (!playerData.ContainsKey(player.playerClientId)) return;

            var data = playerData[player.playerClientId];

            // 메시지 전송
            string message = $"[AntiCheat] {player.playerUsername} 비정상 이동 감지! " +
                           $"거리: {distance:F1}m, 시간: {deltaTime:F2}s, " +
                           $"의심도: {data.SuspicionLevel}/10";

            LethalAntiCheat.Core.MessageUtils.ShowHostOnlyMessage(message);

            // 로그 기록 (BepInEx 로거 사용)
            //Plugin.Logger.LogWarning(message);

            // 의심도가 높으면 킥
            if (data.SuspicionLevel >= 10)
            {
                AntiManager.Instance.KickPlayer(player, "Teleport Hack Detected");
                playerData.Remove(player.playerClientId);
            }
        }

        private struct PlayerTeleportData
        {
            public Vector3 LastPosition;
            public Vector3 LastHelmetPosition;
            public float LastCheckTime;
            public int EmoteShakeCount;
            public int SuspicionLevel;
        }
    }

    // Harmony 패치들
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    public class TeleportDetectorPatch
    {
        private static TeleportDetector detector = new TeleportDetector();

        [HarmonyPostfix]
        public static void Postfix(PlayerControllerB __instance)
        {
            try
            {
                if (__instance != null && NetworkManager.Singleton?.IsHost == true)
                {
                    detector.CheckPlayer(__instance);
                }
            }
            catch (Exception ex)
            {
                //Plugin.Logger.LogError($"[TeleportDetector] Error: {ex.Message}");
            }
        }
    }

    // 이모트 시작/종료 감지
    [HarmonyPatch(typeof(PlayerControllerB), "PerformEmote")]
    public class EmoteDetectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControllerB __instance, int emoteID)
        {
            if (NetworkManager.Singleton?.IsHost == true)
            {
                //Plugin.Logger.LogInfo($"[AntiCheat] {__instance.playerUsername} 이모트 시작: {emoteID}");
            }
        }
    }

    // 플레이어 위치 직접 변경 감지
    [HarmonyPatch(typeof(Transform), "set_position")]
    public class TransformPositionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Transform __instance, Vector3 value)
        {
            // 플레이어 컨트롤러인지 확인
            var player = __instance.GetComponent<PlayerControllerB>();
            if (player != null && NetworkManager.Singleton?.IsHost == true)
            {
                float distance = Vector3.Distance(__instance.position, value);
                if (distance > 20f) // 큰 거리 변경 감지
                {
                   // Plugin.Logger.LogWarning($"[AntiCheat] {player.playerUsername} Transform 직접 변경 감지! 거리: {distance:F1}m");
                }
            }
        }
    }
}