using HarmonyLib;
using LethalHack.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(EnemyAI), "Start")]
    public class ESP : Cheat
    {
        public static List<EnemyAI> enemies = new List<EnemyAI>();

        public override void Trigger()
        {
            // ESP 구현
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.enemyType.name == "") continue;
                if (enemy.enemyType.name == "Doublewing" || enemy.enemyType.name == "DocileLocustBees") continue; // 잡몹은 처리 안하도록 수정
                if (enemy.isEnemyDead) continue; // 적이 죽은 상태면 스킵

                VisualUtil.DrawBoxOutline(enemy.gameObject, Color.red, 2f);
            }
        }

        [HarmonyPostfix]
        public static void FindEnemies(EnemyAI __instance)
        {
            enemies.Add(__instance);
        }
    }
}