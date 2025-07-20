using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    //rpc 핸들러로 클라이언트의 위치를 받아와 동작
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_2013428264")]
    public static class SuperJump
    {
        // 딕셔너리로 플레이어들의 위치 정보를 저장.
        private static readonly Dictionary<ulong, float> lastPlayerYPositions = new Dictionary<ulong, float>();

        //최대 10의 속도
        private const float MAX_VERTICAL_SPEED = 10.0f;

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, FastBufferReader reader, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) 
            { 
                // 허용되지 않은 유저면 차단
                return false; 
            }

            // 위치를 읽어온다.
            reader.ReadValueSafe(out Vector3 newPos);

            // 딕셔너리에 값이 있으면 현재 읽어온 위치와 비교
            if (lastPlayerYPositions.TryGetValue(player.playerSteamId, out float lastY))
            {
                float distanceMoved = newPos.y - lastY;
                float speed = distanceMoved / Time.deltaTime; //속도계산

                if (speed > MAX_VERTICAL_SPEED)
                {
                    AntiManager.Instance.KickPlayer(player, $"Super Jump / Vertical Speed: {speed:F2} m/s");
                    lastPlayerYPositions.Remove(player.playerSteamId); // 킥 했으면 추적 중지
                    return false;
                }
            }

            //조건문 해당 안되면 플레이어별로 위치 읽어오기
            lastPlayerYPositions[player.playerSteamId] = newPos.y;

            return true;
        }
    }
}