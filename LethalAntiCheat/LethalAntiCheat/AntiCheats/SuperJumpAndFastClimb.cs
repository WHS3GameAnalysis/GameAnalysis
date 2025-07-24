using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_2013428264")] // UpdatePlayerPositionServerRpc
    //같은 부분 패치를 다른 클래스에서 동시에 하다보니 멈추는 현상 발생 -> superjump와 fastclimb를 묶음)
    public static class SuperJumpAndFastClimb
    {
        // 각 핵 탐지를 위한 독립적인 추적 딕셔너리 (이전 코드의 lastPlayerYPositions)
        public static readonly Dictionary<ulong, float> climbTracker = new Dictionary<ulong, float>();
        public static readonly Dictionary<ulong, float> jumpTracker = new Dictionary<ulong, float>();

        private const float MAX_CLIMB_SPEED = 50.0f;
        private const float MAX_VERTICAL_SPEED = 108.0f; //기본 점프 속도가 아마 100인듯?

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, FastBufferReader reader, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player))
            {
                // 허용되지 않은 유저면 차단
                return false;
            }
            
            //var startPos = reader.Position;
            reader.ReadValueSafe(out Vector3 newPos);
            //reader.Seek(startPos);

            // 플레이어의 현재 상태에 따라 적절한 로직을 실행
            if (player.isClimbingLadder)
            {
                // 등반 상태일 때: FastClimb 감지
                // 다른 추적기(jumpTracker)에 데이터가 있다면 상태가 전환된 것이므로 삭제
                if (jumpTracker.ContainsKey(player.playerSteamId))
                {
                    jumpTracker.Remove(player.playerSteamId);
                }

                if (climbTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_CLIMB_SPEED)
                    {
                        climbTracker.Remove(player.playerSteamId);
                        return Kick(player, "Fast Climb", speed);
                    }
                }
                climbTracker[player.playerSteamId] = newPos.y;
            }
            else
            {
                // 등반 상태가 아닐 때: SuperJump 감지
                // 다른 추적기(climbTracker)에 데이터가 있다면 상태가 전환된 것이므로 삭제
                if (climbTracker.ContainsKey(player.playerSteamId))
                {
                    climbTracker.Remove(player.playerSteamId);
                }

                if (jumpTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_VERTICAL_SPEED)
                    {
                        jumpTracker.Remove(player.playerSteamId);
                        return Kick(player, "Super Jump", speed);
                    }
                }
                jumpTracker[player.playerSteamId] = newPos.y;
            }
            climbTracker[player.playerSteamId] = newPos.y;
            jumpTracker[player.playerSteamId] = newPos.y;

            return true;
        }

        private static bool Kick(PlayerControllerB player, string hackName, float speed)
        {
            MessageUtils.ShowMessage($"[{hackName}] {player.playerUsername} has abnormal speed: {speed:F2} m/s");
            AntiManager.Instance.KickPlayer(player, $"{hackName} Hack");
            return false;
        }
    }
}