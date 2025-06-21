//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GameNetcodeStuff;
//using HarmonyLib;
//using LethalHack;
//using UnityEngine;
//using static IngamePlayerSettings;

//namespace LethalHack.Cheats
//{
//    [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
//    public class SuperJump : Cheat
//    {
//        public override void Trigger()
//        {
//            // localPlayer가 null이면 초기화
//            if (Hack.localPlayer == null)
//            {
//                Hack.localPlayer = GameNetworkManager.Instance.localPlayerController;
//                if (Hack.localPlayer == null) return;
//            }
//            // jumpForce를 isEnabled 상태에 따라 조정
//            Hack.localPlayer.jumpForce = isEnabled ? 100f : 13f;
//        }

//        [HarmonyPostfix]
//        public static void PlayerJump(PlayerControllerB __instance)
//        {
//            if (Hack.localPlayer == null || __instance == null || Hack.localPlayer != __instance) return;
//            // Trigger()를 호출하여 jumpForce 조정
//            //Hack.Instance.SuperJump.Trigger();
//        }
//    }
//}

using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
    internal class SuperJump : Cheat
    {
        public override void Trigger()
        {
            // 별도 동작 필요 없음. 점프 시에만 적용
        }

        [HarmonyPostfix]
        public static void PlayerJump(PlayerControllerB __instance)
        {
            if (Hack.localPlayer == null || __instance == null || Hack.localPlayer != __instance) return;


            // SuperJump가 켜져 있으면 jumpForce를 100f로, 아니면 원래 값으로 복구
            __instance.jumpForce = Hack.Instance.SuperJump.isEnabled ? 100f : 13f;
        }
    }
}
