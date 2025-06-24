using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
        
        // Kill All 기능 - 임시 해결책 방식으로 구현
        public static void KillAllEnemies()
        {
            if (HUDManager.Instance == null) return;
            
            if (enemies.Count == 0)
            {
                HUDManager.Instance.DisplayTip("LethalHack", "No enemies to kill!");
                return;
            }
            
            int killedCount = 0;
            foreach (EnemyAI enemy in enemies)
            {
                if (enemy != null && !enemy.isEnemyDead)
                {
                    // 직접 Kill 로직 구현
                    if (Hack.localPlayer.IsHost)
                    {
                        if (!enemy.enemyType.canDie) enemy.enemyType.canDie = true;
                        enemy.KillEnemyServerRpc(true);
                    }
                    else 
                    {
                        RoundManager.Instance.DespawnEnemyServerRpc(enemy.GetComponent<NetworkObject>());
                    }
                    killedCount++;
                }
            }
            
            HUDManager.Instance.DisplayTip("LethalHack", $"Killed {killedCount} enemies!");
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