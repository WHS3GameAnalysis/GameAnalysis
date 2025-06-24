using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class EnemyList : Cheat
    {
        public static List<EnemyAI> enemies = new List<EnemyAI>();
        
        public override void Trigger()
        {
            // 매 프레임마다 적 리스트를 업데이트
            UpdateEnemyList();
        }
        
        private void UpdateEnemyList()
        {
            // 현재 맵의 모든 적들을 찾아서 리스트 업데이트
            EnemyAI[] currentEnemies = Object.FindObjectsOfType<EnemyAI>();
            
            // 기존 리스트 클리어
            enemies.Clear();
            
            // 새로운 적들을 리스트에 추가
            foreach (EnemyAI enemy in currentEnemies)
            {
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }
        }
        
        // 적 추가 메서드 (향후 ObjectManager 방식으로 개선 가능)
        public static void AddEnemy(EnemyAI enemy)
        {
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }
        
        // 적 제거 메서드
        public static void RemoveEnemy(EnemyAI enemy)
        {
            if (enemy != null)
            {
                enemies.Remove(enemy);
            }
        }
        
        // 모든 적 제거
        public static void ClearEnemies()
        {
            enemies.Clear();
        }
    }
} 