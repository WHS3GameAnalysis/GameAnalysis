using UnityEngine;
using LethalHack.Cheats;

namespace LethalHack.Util
{
    public class PlayerTab : ITab
    {
        public string TabName => "Player";

        // 별도 창 관련 변수들
        private bool showDamageWindow = false;
        private bool showTeleportWindow = false;
        private Rect damageWindowRect = new Rect(600, 20, 200, 350);
        private Rect teleportWindowRect = new Rect(300, 20, 200, 350);

        public void DrawTab()
        {
            // 체크박스들
            showDamageWindow = GUI.Toggle(new Rect(10, 80, 150, 20), showDamageWindow, "Damage Hack");
            showTeleportWindow = GUI.Toggle(new Rect(10, 105, 150, 20), showTeleportWindow, "Teleport");
            
            // Input Seed (토글 + 입력)
            Hack.Instance.InputSeedHack.isEnabled = GUI.Toggle(new Rect(10, 130, 150, 20), Hack.Instance.InputSeedHack.isEnabled, "Input Seed");
            
            if (Hack.Instance.InputSeedHack.isEnabled)
            {
                GUI.Label(new Rect(30, 155, 80, 20), "Seed:");
                Hack.Instance.InputSeedHack.customSeedText = GUI.TextField(new Rect(110, 155, 100, 20), Hack.Instance.InputSeedHack.customSeedText);
                
                // 입력된 값 표시
                if (!string.IsNullOrEmpty(Hack.Instance.InputSeedHack.customSeedText))
                {
                    GUI.Label(new Rect(30, 180, 200, 20), $"Current Seed: {Hack.Instance.InputSeedHack.customSeedText}");
                }
            }
        }

        public void DrawWindows()
        {
            if (showDamageWindow)
            {
                Hack.Instance.DamageHack.RenderDamageWindow();
            }
            
            if (showTeleportWindow)
            {
                Hack.Instance.TeleportHack.RenderTeleportWindow();
            }
        }
    }
} 