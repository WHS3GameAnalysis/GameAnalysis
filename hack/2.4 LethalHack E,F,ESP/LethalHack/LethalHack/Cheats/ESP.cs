using HarmonyLib;
using LethalHack.Util;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHack.Cheats
{
    [HarmonyPatch]
    public class ESP : Cheat
    {
        public bool ItemESPisEnabled = true;
        public bool EnemyESPisEnabled = true;
        public static List<EnemyAI> enemies = new List<EnemyAI>();
        public static List<GrabbableObject> items = new List<GrabbableObject>();

        public override void Trigger()
        {
            // Trigger에서는 리스트 업데이트만 수행
            UpdateLists();
        }

        private void UpdateLists()
        {
            // null 항목들 제거
            enemies.RemoveAll(enemy => enemy == null);
            items.RemoveAll(item => item == null);
        }

        // GUI 렌더링은 OnGUI에서 호출되어야 함
        public void RenderESP()
        {
            if (EnemyESPisEnabled) EnemyESP();
            if (ItemESPisEnabled) ItemESP();
        }

        public void ItemESP()
        {
            foreach (var item in items)
            {
                if (item == null) continue;

                float distance = CameraUtil.GetDistanceToPlayer(item.transform.position);
                if (distance == 0f || distance > 5000 || !CameraUtil.WorldToScreen(item.transform.position, out var screen)) continue;
                VisualUtil.DrawBoxOutline(item.gameObject, Color.blue, 2f);
                VisualUtil.DrawDistanceString(screen, item.itemProperties.itemName, distance);
            }
        }

        public void EnemyESP()
        {
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
        [HarmonyPatch(typeof(EnemyAI), "Start")]
        public static void FindEnemies(EnemyAI __instance)
        {
            if (!enemies.Contains(__instance))
                enemies.Add(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "Start")]
        public static void FindItems(GrabbableObject __instance)
        {
            if (!items.Contains(__instance))
                items.Add(__instance);
        }
    }
}