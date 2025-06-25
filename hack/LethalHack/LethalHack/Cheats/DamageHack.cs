using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class DamageHack : Cheat
    {
        private Rect windowRect = new Rect(600, 20, 200, 350);
        private String result = ""; // 결과 메시지 저장용

        public override void Trigger()
        {
            // DamageHack은 GUI 기반이므로 Trigger에서는 아무것도 하지 않음
        }

        public void RenderDamageWindow()
        {
            windowRect = GUI.Window(4, windowRect, DamageMenu, "Damage Others");
        }

        private void DamageMenu(int windowID)
        {
            if (GUI.Button(new Rect(10, 30, 180, 30), "Test Self (-10hp)"))
            {
                GameNetworkManager.Instance?.localPlayerController.DamagePlayerFromOtherClientServerRpc(10, new Vector3(), 999);
            }
            var players = GameNetworkManager.Instance?.localPlayerController.playersManager.allPlayerScripts;
            int y = 70;
            foreach (var player in players)
            {
                if (player && !player.isPlayerDead && player != GameNetworkManager.Instance.localPlayerController)
                {
                    // 플레이어 이름과 위치를 표시하는 버튼 생성
                    if (GUI.Button(new Rect(10, y, 180, 30), $"{player.playerUsername} : {player.health}HP"))
                    {
                        player.DamagePlayerFromOtherClientServerRpc(10, new Vector3(), -1);
                    }
                    y += 40; // 다음 버튼 위치 조정
                }
            }

            // 결과 메시지를 표시
            GUI.Label(new Rect(10, y, 180, 60), result);

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }
    }
} 