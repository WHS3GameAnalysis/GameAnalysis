using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 220, 240);
        public bool showMenu = true;

        // 현재 Freecam 상태를 저장
        private bool isFreecamEnabled = false;

        public void Render()
        {
            if (!showMenu) return;

<<<<<<< Updated upstream
            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack"); // GUI 창 생성: ID = 1, 위치 = windowRect, 내용 = DrawMenu 함수
=======
            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack");
>>>>>>> Stashed changes
        }

        private void DrawMenu(int windowID)
        {
            string buttonLabel = isFreecamEnabled ? "Disable Freecam" : "Enable Freecam";

            if (GUI.Button(new Rect(10, 20, 180, 25), buttonLabel))
            {
                isFreecamEnabled = !isFreecamEnabled;

<<<<<<< Updated upstream
            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
=======
                if (isFreecamEnabled)
                {
                    Freecam.Reset(); // ✅ 클래스 이름으로 정적 메서드 호출
                    Freecam.localPlayer = Hack.localPlayer;
                    Freecam.isActive = true;
                }
                else
                {
                    Freecam.isActive = false;
                    Freecam.Stop(); // ✅ 클래스 이름으로 정적 메서드 호출
                }
            }

            GUI.Label(new Rect(10, 55, 200, 20), "Freecam: " + (isFreecamEnabled ? "ON" : "OFF"));
            GUI.Label(new Rect(10, 75, 200, 20), "WASD+QE: 이동");
            GUI.Label(new Rect(10, 95, 200, 20), "Shift: 빠르게 / ESC: 종료");

            GUI.DragWindow();
>>>>>>> Stashed changes
        }
    }
}

