using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using Unity.Netcode;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class GrabObject
    {
        // GrabObjectServerRpc: 클라이언트가 아이템을 집으려고 할 때 호출하는 메서드
        [HarmonyPatch("GrabObjectServerRpc")]
        [HarmonyPrefix]
        public static bool Prefix(PlayerControllerB __instance, NetworkObjectReference grabbedObject)
        {
            // ServerRpc 패치에서 __instance는 RPC를 호출한 클라이언트에 해당하는 서버의 플레이어 객체로 함.
            PlayerControllerB player = __instance;

            // 플레이어 유효성 검사
            if (!AntiCheatUtils.CheckAndGetUser(player.actualClientId, out _))
            {
                return false;
            }

            // 클라이언트가 보내온 네트워크 객체 확인
            if (!grabbedObject.TryGet(out NetworkObject networkObject))
            {
                AntiManager.Instance.KickPlayer(player, "Tried to grab an invalid object.");
                return false; // 비정상적인 잡기 시도를 차단
            }

            GrabbableObject grabbable = networkObject.GetComponent<GrabbableObject>();
            if (grabbable == null)
            {
                // 잡을 수 없는 객체.. ?? 그냥 둔다.
                return true;
            }

            // 아이템 습득 최대 거리
            const float maxGrabDistance = 7.0f;

            // 플레이어와 잡으려는 물체 사이의 거리 계산
            float distance = Vector3.Distance(player.transform.position, grabbable.transform.position);

            // 거리가 최대 허용치보다 멀 경우 핵으로 간주
            if (distance > maxGrabDistance)
            {
                AntiManager.Instance.KickPlayer(player, $"Inappropriate Grabbing (Distance: {distance:F2}m)");
                return false; 
            }

            return true;
        }
    }
}