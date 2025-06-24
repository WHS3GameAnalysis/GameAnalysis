using HarmonyLib;
using LethalHack.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static IngamePlayerSettings;

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

                float distance = CameraUtil.GetDistanceToPlayer(enemy.transform.position);
                if (distance == 0f || distance > 5000 || !CameraUtil.WorldToScreen(enemy.transform.position, out var screen)) continue;
                VisualUtil.DrawBoxOutline(enemy.gameObject, Color.red, 2f);
                VisualUtil.DrawDistanceString(screen, enemy.enemyType.name, distance);
            }
        }

        [HarmonyPostfix]
        public static void FindEnemies(EnemyAI __instance)
        {
            enemies.Add(__instance);
        }
    }
}