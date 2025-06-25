using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack.Util
{
    public class SelfTab : ITab
    {
        public string TabName => "Self";

        public void DrawTab()
        {
            // 플레이어 자신의 능력 관련 토글들
            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, 80, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            Hack.Instance.SuperJump.isEnabled = GUI.Toggle(new Rect(10, 105, 180, 20), Hack.Instance.SuperJump.isEnabled, "Super Jump");
            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, 130, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");
            Hack.Instance.FastClimbHack.isEnabled = GUI.Toggle(new Rect(10, 155, 180, 20), Hack.Instance.FastClimbHack.isEnabled, "Fast Climb");
            
            // 아직 구현되지 않은 기능들 (예정)
            GUI.Label(new Rect(10, 180, 180, 20), "Noclip (예정)");
        }

        public void DrawWindows()
        {
            // Self 탭은 별도 창이 없으므로 빈 구현
        }
    }
} 