using HarmonyLib;
using Mono.Security.Authenticode;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch(typeof(EnemyAI), "Start")]
    public class ESP : Cheat
    {
        public static List<EnemyAI> enemies = new List<EnemyAI>();

        public override void Trigger()
        {
            GUI.Label(new Rect(10, 40, 400, 20), $"ESP Debug: {enemies.Count} enemies");

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy != null)
                    GUI.Label(new Rect(10, 60 + i * 20, 400, 20), $"Enemy {i}: {enemy.name}");
            }
        }

        [HarmonyPostfix]
        public static void FindEnemies(EnemyAI __instance)
        {
            enemies.Add(__instance);
        }
    }
}