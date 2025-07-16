using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(ShipTeleporter))]
    public class ShipTeleporter
    {
        [HarmonyPatch("PressTeleportButtonServerRpc")]
        [HarmonyPrefix]
        public static bool Prefix(ShipTeleporter __instance, __RpcParams rpcParams)
        {
            // RPC 클라이언트 유효성 확인?
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player))
            {
                return true; // 유효하지 않은 플레이어는 CheckAndGetUser에서 처리되므로, 여기서는 원본 메서드 실행을 허용.->Core.AntiCheatUtil로
            }

            // 텔레포터 쿨타임 중인데 사용되면 핵으로 간주
            if (Time.time < Traverse.Create(__instance).Field("cooldownTime").GetValue<float>())
            {
                AntiManager.Instance.KickPlayer(player, "Inappropriate Teleport");

                return false;
            }

            return true;
        }
    }
}
