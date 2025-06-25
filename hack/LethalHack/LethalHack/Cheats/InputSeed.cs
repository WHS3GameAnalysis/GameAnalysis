using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    public class InputSeed : Cheat
    {
        public string customSeedText = ""; // 입력된 시드 텍스트 저장
        public int customSeed = 0; // 파싱된 시드 값

        public override void Trigger()
        {
            // Trigger에서는 입력값 파싱만 수행
            if (isEnabled && !string.IsNullOrEmpty(customSeedText))
            {
                if (int.TryParse(customSeedText, out int parsedSeed))
                {
                    customSeed = parsedSeed;
                }
            }
        }

        [HarmonyPrefix]
        public static bool StartGamePrefix(StartOfRound __instance)
        {
            if (Hack.Instance.InputSeedHack.isEnabled && Hack.Instance.InputSeedHack.customSeed > 0)
            {
                // 커스텀 시드 적용
                int customSeed = Hack.Instance.InputSeedHack.customSeed;
                __instance.randomMapSeed = customSeed;
                __instance.overrideRandomSeed = true;
                __instance.overrideSeedNumber = customSeed;

                Debug.Log($"[InputSeed] Custom seed applied: {customSeed}");
                return true;
            }

            return true;
        }
    }
} 