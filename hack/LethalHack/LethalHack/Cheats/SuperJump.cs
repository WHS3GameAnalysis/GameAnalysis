using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using LethalHack;
using UnityEngine;
using static IngamePlayerSettings;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
    public class SuperJump : Cheat
    {
        public override void Trigger()
        {
            // localPlayer가 null이면 초기화
            if (hack.localPlayer == null)
            {
                hack.localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (hack.localPlayer == null) return;
            }
            // jumpForce를 isEnabled 상태에 따라 조정
            hack.localPlayer.jumpForce = isEnabled ? 100f : 5f;
        }

        [HarmonyPostfix]
        public static void PlayerJump(PlayerControllerB __instance)
        {
            if (hack.localPlayer == null || __instance == null || hack.localPlayer != __instance) return;
            // Trigger()를 호출하여 jumpForce 조정
            Hack.Instance.SuperJump.Trigger();
        }
    }
}
