
using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    internal static class Teleporter
    {
        //원격으로 텔레포트하는 경우(발판 위에 있지 않고 텔레포트, 버튼 안스고 텔레포트 등.)
        public static bool IsPlayerNearObject(PlayerControllerB player, Transform targetObject, float maxDistance, string cheatName)
        {
            if (player == null || targetObject == null) return false;

            float distance = Vector3.Distance(player.transform.position, targetObject.position);
            if (distance > maxDistance)
            {
                AntiManager.Instance.KickPlayer(player, $"Remote {cheatName} Teleport Usage");
                return false;
            }
            return true;
        }
    }

    // 버튼 조작 가능한 거리 안에서 텔레포트 했는지(버튼을 누를 수 있는 사람이 텔레포트 했는가?)
    [HarmonyPatch(typeof(ShipTeleporter), "__rpc_handler_389447712")]
    public static class ShipTeleporterButton
    {
        private const float MAX_INTERACTION_DISTANCE = 9f; //최대 거리 7, 허용치 9.

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) { return false; }

            var teleporter = __instance as ShipTeleporter;
            if (teleporter == null) return false;

            // 원격으로 사용한 경우
            if (!Teleporter.IsPlayerNearObject(player, teleporter.buttonTrigger.transform, MAX_INTERACTION_DISTANCE, "Teleporter"))
            {
                return false; 
            }

            // 쿨타임 없이 사용하는 경우
            if (Time.time < Traverse.Create(teleporter).Field("cooldownTime").GetValue<float>())
            {
                AntiManager.Instance.KickPlayer(player, "Teleport before Cooldown");
                return false;
            }

            return true; //둘 다 아니면 통과
        }
    }

    //역방향 텔레포트 경우
    [HarmonyPatch(typeof(ShipTeleporter), "__rpc_handler_3033548568")]
    public static class InverseTeleporter
    {
        private const float MAX_PAD_DISTANCE = 10f; //텔레포트 장판 위가 아닌데 텔레포트 하는 경우 탐지. 최대 8, 허용치 10

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) { return false; }
            
            return Teleporter.IsPlayerNearObject(player, __instance.transform, MAX_PAD_DISTANCE, "Inverse Teleporter");
        }
    }

    // 시설 입구->시설 안으로 텔레포트 하는데 거리가 되는지.
    [HarmonyPatch(typeof(EntranceTeleport), "__rpc_handler_4279190381")]
    public static class EntranceTeleport_Patch
    {
        private const float MAX_DOOR_DISTANCE = 14f; // 문 입구랑 최대 거리. 최대 12, 허용 14.

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) { return false; }

            return Teleporter.IsPlayerNearObject(player, __instance.transform, MAX_DOOR_DISTANCE, "Entrance");
        }
    }
}
