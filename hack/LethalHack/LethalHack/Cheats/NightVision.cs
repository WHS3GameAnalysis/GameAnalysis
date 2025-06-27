using UnityEngine;
using GameNetcodeStuff;

namespace LethalHack.Cheats
{
    public class NightVision : Cheat
    {
        public override void Trigger()
        {
            if (Hack.localPlayer == null) return;
            
            if (isEnabled)
            {
                Hack.localPlayer.nightVision.enabled = true;
                Hack.localPlayer.nightVision.intensity = 300.0f; // 밝기
                Hack.localPlayer.nightVision.range = 300.0f; // 범위
                Hack.localPlayer.nightVision.color = Color.blue;
            }
            else
            {
                Hack.localPlayer.nightVision.enabled = false;
            }
        }
    }
}