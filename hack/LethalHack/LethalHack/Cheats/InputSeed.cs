using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(DungeonGenerator), "OuterGenerate")]
    public class InputSeed : Cheat
    {
        public int selectedSeedIndex=0; //시드 인덱스 선택해서 적용합니다
        //각각 지정된 시드 번호와 이름입니다(입력 구현 전에 어느 자리수까지 적용되는지 디버깅 용도)
        public static readonly int[] seedListNumber =
        {
            12345,
            123456,
            1234567,
            12345678,
            123456789,
            1234567890
        };
        public static readonly string[] seedListName =
        {
            "option 1 - (5)",
            "option 2 - (6)",
            "option 3 - (7)",
            "option 4 - (8)",
            "option 5 - (9)",
            "option 6 - (10)"
        };
        public override void Trigger()
        {
            //하모니로 작동
        }

        [HarmonyPrefix]
        public static bool OuterGeneratePrefix(DungeonGenerator __instance, ref int ___ChosenSeed)
        {
            if (Hack.Instance.Seed.isEnabled)
            {
                // 인덱스 번호 시드 선택
                int customSeed = seedListNumber[Hack.Instance.Seed.selectedSeedIndex];

                // DungeonGenerator의 seed 관련 필드들 패치
                __instance.Seed = customSeed;
                __instance.ShouldRandomizeSeed = false; // 랜덤화 비활성화

                Debug.Log($"[InputSeed] Custom seed applied: {customSeed}");
                //커스텀 시드가 없으면 자체 생성하는 기능이 있었음

                return true;
            }

            return true;
        }
    }
}
