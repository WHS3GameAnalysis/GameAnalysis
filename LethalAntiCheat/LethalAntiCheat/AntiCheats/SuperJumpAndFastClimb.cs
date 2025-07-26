using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
// using System.Timers; // Unity의 Time.time을 사용하므로 불필요
// using System.Runtime.Remoting.Messaging; // 불필요
// using System; // 불필요

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_2013428264")] // UpdatePlayerPositionServerRpc
    //같은 부분 패치를 다른 클래스에서 동시에 하다보니 멈추는 현상 발생 -> superjump와 fastclimb를 묶음)
    public static class SuperJumpAndFastClimb
    {
        // 각 핵 탐지를 위한 독립적인 추적 딕셔너리 (이전 코드의 lastPlayerYPositions)
        private static readonly Dictionary<ulong, float> climbTracker = new Dictionary<ulong, float>();
        private static readonly Dictionary<ulong, float> jumpTracker = new Dictionary<ulong, float>();
        // FastClimb 오탐 방지를 위해 등반 시작 시간을 기록하는 딕셔너리
        private static readonly Dictionary<ulong, float> climbStartTimes = new Dictionary<ulong, float>();

        private const float MAX_CLIMB_SPEED = 13.0f;
        private const float MAX_VERTICAL_SPEED = 108.0f; //점프 속도가 아마 100인듯?
        // FastClimb 감지를 수행할 시간 (초). 이 시간 이후에는 사다리 끝 도달 시의 급가속으로 인한 오탐을 방지하기 위해 감지를 중단.
        private const float CLIMB_DETECTION_DURATION = 0.3f;

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, FastBufferReader reader, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) { return true; }

            var startPos = reader.Position;
            reader.ReadValueSafe(out Vector3 newPos);
            reader.Seek(startPos);

            // 플레이어의 현재 상태에 따라 적절한 로직을 실행
            if (player.isClimbingLadder)
            {
                // 등반 상태일 때: FastClimb 감지
                // 다른 추적기(jumpTracker)에 데이터가 있다면 상태가 전환된 것이므로 삭제
                if (jumpTracker.ContainsKey(player.playerSteamId))
                {
                    jumpTracker.Remove(player.playerSteamId);
                }

                // 사다리를 타기 시작한 시간을 아직 기록하지 않았다면, 현재 시간을 기록
                if (!climbStartTimes.ContainsKey(player.playerSteamId))
                {
                    climbStartTimes[player.playerSteamId] = Time.time;
                }

                // 등반 시작 후 0.3초가 지났다면, 오탐 방지를 위해 더 이상 감지하지 않고 정상 처리
                if (climbStartTimes.TryGetValue(player.playerSteamId, out float startTime) && Time.time - startTime > CLIMB_DETECTION_DURATION)
                {
                    return true;
                }

                // 0.3초 이내일 경우에만 속도 감지 수행
                if (climbTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_CLIMB_SPEED)
                    {
                        return Kick(player, "Fast Climb", speed, climbTracker);
                    }
                }
                // 현재 위치를 다음 프레임의 계산을 위해 저장
                climbTracker[player.playerSteamId] = newPos.y;
            }
            else
            {
                // 등반 상태가 아닐 때: SuperJump 감지 및 등반 관련 데이터 초기화
                if (climbTracker.ContainsKey(player.playerSteamId))
                {
                    climbTracker.Remove(player.playerSteamId);
                }
                if (climbStartTimes.ContainsKey(player.playerSteamId))
                {
                    climbStartTimes.Remove(player.playerSteamId);
                }

                if (jumpTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_VERTICAL_SPEED)
                    {
                        return Kick(player, "Super Jump / teleport", speed, jumpTracker);
                    }
                }
                // 현재 위치를 다음 프레임의 계산을 위해 저장
                jumpTracker[player.playerSteamId] = newPos.y;
            }
            return true;
        }

        private static bool Kick(PlayerControllerB player, string hackName, float speed, Dictionary<ulong, float> tracker)
        {
            MessageUtils.ShowMessage($"[{hackName}] {player.playerUsername} has abnormal speed: {speed:F2} m/s");
            AntiManager.Instance.KickPlayer(player, $"{hackName} Hack");
            tracker.Remove(player.playerSteamId); // 해당 추적기에서 플레이어 제거
            
            // 핵 감지 시, 등반 시작 시간 기록도 함께 제거
            if (climbStartTimes.ContainsKey(player.playerSteamId))
            {
                climbStartTimes.Remove(player.playerSteamId);
            }

            return false; // 위치 업데이트 차단
        }
    }
}

