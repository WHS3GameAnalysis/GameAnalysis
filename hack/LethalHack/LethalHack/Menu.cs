using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack
{
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 220, 200); // 크기 약간 확대
        public bool showMenu = true;

        public void Render()
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack");
        }

        private void DrawMenu(int windowID)
        {
          
            // 기존 기능 토글
            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, 20, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, 45, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");
           // Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, 70, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HPDisplay");

            // Freecam 버튼
            string buttonLabel = Freecam.isActive ? "Disable Freecam" : "Enable Freecam";
            if (GUI.Button(new Rect(10, 100, 180, 25), buttonLabel))
            {
                if (!Freecam.isActive)
                {
                    Freecam.Reset();
                    Freecam.isActive = true;
                    Hack.Instance.freecam.isEnabled = true;
                }
                else
                {
                    Freecam.isActive = false;
                    Freecam.Stop();
                }
            }

            // 상태 안내
            GUI.Label(new Rect(10, 135, 200, 20), "Freecam: " + (Freecam.isActive ? "ON" : "OFF"));

            GUI.DragWindow(); // 드래그는 반드시 마지막에!
        }
    }
}
