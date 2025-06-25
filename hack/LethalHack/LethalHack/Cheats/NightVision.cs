using UnityEngine;
using GameNetcodeStuff;

namespace LethalHack
{
    internal class NightVision
    {
        public static bool isEnabled = true;



        public static void Trigger()
        {
            //PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            //if (player == null) return;

            if (Hack.localPlayer == null) return;
            
            Hack.localPlayer.nightVision.enabled = true;
            Hack.localPlayer.nightVision.intensity = 300.0f; // 밝기
            Hack.localPlayer.nightVision.range = 300.0f; // 범위
            Hack.localPlayer.nightVision.color = Color.blue;
        }
        /* 
        //버튼으로 추가
        public static void Enable()
        {
            isEnabled = true;
        }

        public static void Disable()
        {
            isEnabled = false;

            
            if (Hack.localPlayer != null)
            {
                Hack.localPlayer.nightVision.enabled = false;
            }
        }*/
    }
}