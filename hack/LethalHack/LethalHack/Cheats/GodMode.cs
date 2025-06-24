using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;


namespace LethalHack.Cheats
{
    [HarmonyPatch]
    public class GodMode : Cheat // Cheat 클래스를 상속
    {
        public static GodMode Instance;

        public GodMode()
        {
            Instance = this;
        }

        public override void Trigger() // 실제 기능을 구현한 메서드
        {
            if (Hack.localPlayer == null)
            {
                Hack.localPlayer = GameNetworkManager.Instance.localPlayerController; // 플레이어 객체를 로컬 플레이어(자기 자신?)로 초기화
                if (Hack.localPlayer == null) return; // 초기화 안됐을 경우에 return
            }

            Hack.localPlayer.health = 100; // 체력 100으로 설정
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        public static bool NoDamage()
        {
            return !Instance.isEnabled;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        public static bool NoKill()
        {
            return !Instance.isEnabled;
        }
    }
}