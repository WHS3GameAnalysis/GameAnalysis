using UnityEngine;

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 200, 170); // 창 위치, 크기 지정
        public bool showMenu = true;

        public void Render()
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack");
        }

        private void DrawMenu(int windowID)
        {
            int y = 20;

            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, y, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            y += 25;

            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, y, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");
            y += 25;

            Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, y, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HP Display");
            y += 25;

            Hack.Instance.SuperJump.isEnabled = GUI.Toggle(new Rect(10, y, 180, 20), Hack.Instance.SuperJump.isEnabled, "Super Jump");
            y += 25;

            Hack.Instance.NoClip.isEnabled = GUI.Toggle(new Rect(10, y, 180, 20), Hack.Instance.NoClip.isEnabled, "NoClip"); 

            GUI.DragWindow(); // 창 이동 가능
        }
    }
}
