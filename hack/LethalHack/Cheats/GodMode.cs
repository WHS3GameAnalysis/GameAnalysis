using GameNetcodeStuff;
using UnityEngine;


namespace LethalHack.Cheats
{
    public class GodMode : Cheat // Cheat 클래스를 상속
    {
        public static PlayerControllerB localPlayer; // 스탯을 조작하기 위해 사용할 플레이어 객체

        public override void Trigger() // 실제 기능을 구현한 메서드
        {
            if (GodMode.localPlayer == null)
            {
                GodMode.localPlayer = GameNetworkManager.Instance.localPlayerController; // 플레이어 객체를 로컬 플레이어(자기 자신?)로 초기화
                if (GodMode.localPlayer == null) return; // 초기화 안됐을 경우에 return
            }

            GodMode.localPlayer.health = 100; // 체력 100으로 설정
        }
    }
}