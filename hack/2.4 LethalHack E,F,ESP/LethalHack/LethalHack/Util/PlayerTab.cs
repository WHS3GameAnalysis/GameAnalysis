using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack.Util
{
    public class PlayerTab : ITab
    {
        public string TabName => "Player";

        public void DrawTab()
        {
            // 다른 플레이어 관련 기능들 (아직 구현되지 않은 기능들)
            GUI.Label(new Rect(10, 80, 180, 20), "Teleport (예정)");
            GUI.Label(new Rect(10, 105, 180, 20), "Damage (예정)");
            GUI.Label(new Rect(10, 130, 180, 20), "InputSeed (예정)");
        }

        public void DrawWindows()
        {
            // Player 탭은 별도 창이 없으므로 빈 구현
        }
    }
} 