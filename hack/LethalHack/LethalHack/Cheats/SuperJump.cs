using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
    internal class SuperJump : Cheat
    {
        public override void Trigger()
        {
            // 별도 동작 필요 없음. 점프 시에만 적용
        }

        [HarmonyPostfix]
        public static void PlayerJump(PlayerControllerB __instance)
        {
            if (Hack.localPlayer == null || __instance == null || Hack.localPlayer != __instance) return;


            // SuperJump가 켜져 있으면 jumpForce를 100f로, 아니면 원래 값으로 복구
            __instance.jumpForce = Hack.Instance.SuperJump.isEnabled ? 100f : 13f;
        }
    }
}
