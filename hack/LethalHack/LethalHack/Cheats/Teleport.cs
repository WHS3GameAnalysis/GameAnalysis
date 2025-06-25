using GameNetcodeStuff;
using System;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication.Internal;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class TeleportImpl
    {
        private static Vector3 savedPosition = Vector3.zero;
        private static bool hasSavedPosition = false;

        public static string SavePosition()
        {
            try
            {
                PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
                if (localPlayer != null)
                {
                    savedPosition = localPlayer.transform.position;
                    hasSavedPosition = true;
                    return $"위치 저장: ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})";
                }
                else
                {
                    return "플레이어를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                return $"위치 저장 실패: {ex.Message}";
            }
        }

        public static string LoadPosition()
        {
            try
            {
                if (!hasSavedPosition)
                {
                    return "저장된 위치가 없습니다.";
                }

                PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
                if (localPlayer != null)
                {
                    localPlayer.transform.position = savedPosition;
                    return $"위치 로드: ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})";
                }
                else
                {
                    return "플레이어를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                return $"위치 로드 실패: {ex.Message}";
            }
        }

        public static string TeleportTo(Vector3 targetPos)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
            if (localPlayer != null)
            {
                localPlayer.transform.position = targetPos;
                return $"텔레포트 완료: ({targetPos.x:F2}, {targetPos.y:F2}, {targetPos.z:F2})";
            }

            return "플레이어를 찾을 수 없습니다.";
        }
    }

    public class Teleport : Cheat
    {
        private Rect windowRect = new Rect(300, 20, 200, 350);
        private String result = "결과 MSG"; // 결과 메시지 저장용

        public override void Trigger()
        {
            // Teleport는 GUI 기반이므로 Trigger에서는 아무것도 하지 않음
        }

        public void RenderTeleportWindow()
        {
            windowRect = GUI.Window(5, windowRect, TeleportMenu, "Teleport");
        }

        private void TeleportMenu(int windowID)
        {
            if (GUI.Button(new Rect(10, 30, 80, 30), "Save Pos"))
            {
                result = TeleportImpl.SavePosition();
            }
            if (GUI.Button(new Rect(110, 30, 80, 30), "Load Pos"))
            {
                result = TeleportImpl.LoadPosition();
            }
            var players = GameNetworkManager.Instance?.localPlayerController.playersManager.allPlayerScripts;
            int y = 70;
            foreach (var player in players)
            {
                if (player && !player.isPlayerDead && player != GameNetworkManager.Instance.localPlayerController)
                {
                    // 플레이어 이름과 위치를 표시하는 버튼 생성
                    if (GUI.Button(new Rect(10, y, 180, 30), $"{player.playerUsername} ({player.transform.position.x:F2}, {player.transform.position.y:F2}, {player.transform.position.z:F2})"))
                    {
                        result = TeleportImpl.TeleportTo(player.transform.position);
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