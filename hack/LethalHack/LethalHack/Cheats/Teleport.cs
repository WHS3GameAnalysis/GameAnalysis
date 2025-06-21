using GameNetcodeStuff;
using System;
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
    }

    public class Teleport : Cheat // Cheat 클래스 상속
    {
        private Rect windowRect = new Rect(300, 20, 200, 150);
        private String result = ""; // 결과 메시지 저장용

        public void Render() // Cheat 클래스 MonoBehaviour 상속받았음 -> Render 메서드 자동으로 호출됨
        {
            windowRect = GUI.Window(2, windowRect, TeleportMenu, "Teleport");
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

            // 결과 메시지를 표시
            GUI.Label(new Rect(10, 70, 180, 60), result);

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }

        public override void Trigger()
        {
            throw new NotImplementedException();
        }
    }
}
