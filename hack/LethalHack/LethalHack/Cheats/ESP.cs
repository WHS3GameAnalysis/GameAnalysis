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
            //GUI.Label(new Rect(10, 40, 400, 20), $"ESP Debug: {enemies.Count} enemies");

            //for (int i = 0; i < enemies.Count; i++)
            //{
            //    var enemy = enemies[i];
            //if (enemy != null)
            //    GUI.Label(new Rect(10, 60 + i * 20, 400, 20), $"Enemy {i}: {enemy.name}");
            //}

            // ESP 구현
            foreach (var enemy in enemies)
            {
                //if (enemy == null | enemy.enemyType.name == "") continue;

                if (Hack.localPlayer.gameplayCamera == null) // 디버깅 용
                    GUI.Label(new Rect(30, 100, 400, 20), "gameplayCamera is null!!!");

                Vector3 worldPos = enemy.transform.position;
                Vector3 screenPos = Hack.localPlayer.gameplayCamera.WorldToScreenPoint(worldPos);
                //GUI.Label(new Rect(10, 125, 400, 20), $"screenPos: {screenPos}"); // 디버깅 용
                if (screenPos.z < 0) continue; // 카메라 뒤에 있으면 무시

                float screenY = Screen.height - screenPos.y;

                float boxWidth = 60f;
                float boxHeight = 120f;

                GUI.Label(new Rect(10, 200, 400, 20), $"Type: {enemy.enemyType.name}");

                Rect box = new Rect(
                    screenPos.x - boxWidth / 2,
                    screenY - boxHeight / 2,
                    boxWidth,
                    boxHeight
                    );

                GUI.color = Color.red;
                GUI.Box(box, "Enemy");
            }
        }

        [HarmonyPostfix]
        public static void FindEnemies(EnemyAI __instance)
        {
            enemies.Add(__instance);
        }
    }
}