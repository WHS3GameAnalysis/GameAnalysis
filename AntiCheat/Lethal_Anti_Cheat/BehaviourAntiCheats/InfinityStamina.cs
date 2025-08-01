
/*
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Lethal_Anti_Cheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    public static class InfinityStamina
    {
        private static readonly Dictionary<PlayerControllerB, float> sprintStartTimes = new Dictionary<PlayerControllerB, float>();
        private const float SPRINT_CHECK_DURATION = 5.0f;
        private const float STAMINA_THRESHOLD = 0.8f;

        [HarmonyPostfix]
        public static void Postfix(PlayerControllerB __instance)
        {
            if (!__instance.isPlayerControlled)
            {
                if (sprintStartTimes.ContainsKey(__instance))
                {
                    sprintStartTimes.Remove(__instance);
                }
                return;
            }

            if (__instance.isSprinting)
            {
                if (!sprintStartTimes.ContainsKey(__instance))
                {
                    sprintStartTimes.Add(__instance, Time.time);
                }
                else
                {
                    float sprintStartTime = sprintStartTimes[__instance];
                    if (Time.time - sprintStartTime >= SPRINT_CHECK_DURATION)
                    {
                        if (__instance.sprintMeter > STAMINA_THRESHOLD)
                        {
                            AntiManager.KickPlayer(__instance, "Infinite Stamina");
                            sprintStartTimes.Remove(__instance);
                        }
                    }
                }
            }
            else
            {
                if (sprintStartTimes.ContainsKey(__instance))
                {
                    sprintStartTimes.Remove(__instance);
                }
            }
        }
    }
}

*/ //아예 완전 달리기 시간 초과하도록 수정
