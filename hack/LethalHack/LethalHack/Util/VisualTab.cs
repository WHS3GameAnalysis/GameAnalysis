using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack.Util
{
    public class VisualTab : ITab
    {
        public string TabName => "Visual";

        public void DrawTab()
        {
            // 시각적 효과 관련 토글들
            Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, 80, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HPDisplay");
            Hack.Instance.NoVisorHack.isEnabled = GUI.Toggle(new Rect(10, 105, 180, 20), Hack.Instance.NoVisorHack.isEnabled, "NoVisor (헬멧 없애기)");
            
            // ESP 기능들
            Hack.Instance.ESPHack.ItemESPisEnabled = GUI.Toggle(new Rect(10, 130, 180, 20), Hack.Instance.ESPHack.ItemESPisEnabled, "ESP (ITEM)");
            Hack.Instance.ESPHack.EnemyESPisEnabled = GUI.Toggle(new Rect(10, 155, 180, 20), Hack.Instance.ESPHack.EnemyESPisEnabled, "ESP (Enemy)");
            
            // Minimap 토글
            Hack.Instance.MinimapHack.isEnabled = GUI.Toggle(new Rect(10, 180, 180, 20), Hack.Instance.MinimapHack.isEnabled, "Minimap");
        }

        public void DrawWindows()
        {
            // Visual 탭은 별도 창이 없으므로 빈 구현
        }
    }
} 