﻿using GameNetcodeStuff;
using UnityEngine;


namespace LethalHack.Cheats
{
    public class InfinityStamina : Cheat // Cheat 클래스 상속
    {

        public override void Trigger() // 기능을 구현할 메서드
        {
            if (Hack.localPlayer == null)
            {
                Hack.localPlayer = GameNetworkManager.Instance.localPlayerController; // 플레이어 객체 로컬 플레이어로 초기화
                if (Hack.localPlayer == null) return; // 초기화 실패시 return
            }

            Hack.localPlayer.sprintMeter = 1f; // 스태미나 100%로 설정
        }
    }
}