using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.Core;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class SuperJump
    {
        [HarmonyPatch("PlayerJump")]
        [HarmonyPrefix]
        public static bool Prefix(PlayerControllerB __instance)
        {
            // 기본 점프 13f, 20f 부터 핵으로 간주한다.
            const float maxAllowedJumpForce = 20f;

            if (__instance.jumpForce >= maxAllowedJumpForce)
            {
                AntiManager.Instance.KickPlayer(__instance, "Super Jump");

                // 원본 PlayerJump 메서드의 실행을 막아 비정상적인 점프를 차단
                return false;
            }

            return true;
        }
    }
}