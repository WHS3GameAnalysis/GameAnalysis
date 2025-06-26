using GameNetcodeStuff;
using LethalHack.Cheats;
using LethalHack.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static IngamePlayerSettings;

namespace LethalHack.Manager
{
    internal static class GameUtil
    {
        public static void RenderPlayerModels()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (localPlayer == null)
                return;

            // FreeCam이 활성화된 경우, 로컬 플레이어 모델을 끔
            if (Freecam.isActive)
            {
                localPlayer.DisablePlayerModel(localPlayer.gameObject, true);
                localPlayer.thisPlayerModelArms.enabled = false;
                return;
            }

            localPlayer.DisablePlayerModel(localPlayer.gameObject, false, true);
            localPlayer.thisPlayerModelArms.enabled = true;

            foreach (PlayerControllerB player in localPlayer.playersManager.allPlayerScripts)
            {
                if (localPlayer.playerClientId == player.playerClientId)
                    continue;

                player.DisablePlayerModel(player.gameObject, false, true);
                player.thisPlayerModelArms.enabled = true;
            }
        }
    }
}
