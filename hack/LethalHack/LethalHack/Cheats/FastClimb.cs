using GameNetcodeStuff;
using UnityEngine;
namespace LethalHack.Cheats
{
    public class FastClimb : Cheat // Cheat 클래스를 상속
    {
        private static float defaultClimbSpeed = -1f; // 기본 등반 속도를 저장할 변수
        public static float fastClimbSpeed = 20f;     // 빠른 등반 속도
        public override void Trigger()
        {
            if (Hack.localPlayer == null)
            {
                Hack.localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (Hack.localPlayer == null) return;
            }
            // 기본 등반 속도를 저장 (처음 한 번만)
            if (defaultClimbSpeed == -1f)
                defaultClimbSpeed = Hack.localPlayer.climbSpeed;
            // 속도 토글
            if (Mathf.Approximately(Hack.localPlayer.climbSpeed, defaultClimbSpeed))
                Hack.localPlayer.climbSpeed = fastClimbSpeed;
            else
                Hack.localPlayer.climbSpeed = defaultClimbSpeed;
        }
    }
}