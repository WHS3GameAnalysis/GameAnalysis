using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace LethalAntiCheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    public static class InfinityStamina
    {
        private static readonly Dictionary<PlayerControllerB, (float lastStamina, int kickStack)> suspiciousPlayers = new Dictionary<PlayerControllerB, (float, int)>();
        //의심 플레이어(플레이어, (스테미나 변수, 스택 변수)) 딕셔너리 객체 생성.
        [HarmonyPostfix]
        public static void Postfix(PlayerControllerB __instance)
        {
            //플레이어가 뛰고 있다면 의심 플레이어 목록에 추가
            if (!__instance.isPlayerControlled || !__instance.isSprinting)
            {
                if (suspiciousPlayers.ContainsKey(__instance))
                {
                    suspiciousPlayers.Remove(__instance);
                }
                return;
            }

            //의심 플레이어의 틱당 전/후 스테미너 값을 비교해서 0.05 미만으로 차이나면 의심 스택 증가.(게임 구조상 뛸 때 -0.08씩 update)
            if (suspiciousPlayers.TryGetValue(__instance, out var record))
            {
                if (Mathf.Abs(record.lastStamina - __instance.sprintMeter) < 0.05f)
                {
                    record.kickStack++;
                    //의심 스택이 11 이상이면 추방
                    if (record.kickStack > 10)
                    {
                        AntiManager.Instance.KickPlayer(__instance, "Infinite Stamina (Suspicious Activity)");
                        suspiciousPlayers.Remove(__instance);
                    }
                    else
                    {
                        suspiciousPlayers[__instance] = (__instance.sprintMeter, record.kickStack);
                    }
                }
                //의심이 풀리면 의심 스택을 0으로 초기화
                else
                {
                    suspiciousPlayers[__instance] = (__instance.sprintMeter, 0);
                }
            }
            //아주 아무것도 없으면 오류 방지를 위해 0으로 초기화.
            else
            {
                suspiciousPlayers.Add(__instance, (__instance.sprintMeter, 0));
            }
        }
    }
}
