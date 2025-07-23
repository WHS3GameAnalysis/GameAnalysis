using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;
using Unity.Netcode;

namespace LethalAntiCheat.AntiCheats
{
    internal static class GodMode
    {
        public static void CheckForGodMode(PlayerControllerB victim)
        {
            // 이미 죽은 경우 넘긴다.
            if (victim == null || victim.isPlayerDead)
            {
                return;
            }

            // 피해를 입었으나 100 이상이면 핵 사용자로 간주.
            if (victim.health >= 100)
            {
                AntiManager.Instance.KickPlayer(victim, "God Mode");
            }
        }
    }

    // 다른 플레이어에게 데미지를 입은 경우
    //DamagePlayerFromOtherClientServerRpc.
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_638895557")]
    public static class GodMode_PlayerDamage
    {
        [HarmonyPostfix]
        public static void Postfix(NetworkBehaviour __instance)
        {
            GodMode.CheckForGodMode(__instance as PlayerControllerB);
        }
    }

    // 다른 무언가로부터 데미지를 입은 경우
    //DamagePlayerServerRpc.
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_1084949295")]
    public static class GodMode_SelfDamage
    {
        [HarmonyPostfix]
        public static void Postfix(NetworkBehaviour __instance)
        {
            GodMode.CheckForGodMode(__instance as PlayerControllerB);
        }
    }
}