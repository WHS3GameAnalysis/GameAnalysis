using UnityEngine;
using GameNetcodeStuff;

namespace LethalHack
{
    internal class NightVision
    {
        public static bool isEnabled = true;

        

        public static void Trigger()
        {
            PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
            if (player == null) return;

            // 나만 적용
            player.nightVision.enabled = true;
            player.nightVision.intensity = 80.0f; // 밝기
            player.nightVision.range = 100.0f; // 범위
            player.nightVision.color = Color.blue;
        }

        public static void Enable()
        {
            isEnabled = true;
        }

        public static void Disable()
        {
            isEnabled = false;

            // 내 나이트비전 끄기
            PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
            if (player != null)
            {
                player.nightVision.enabled = false;
            }
        }
    }
}