using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class EnemySpawn : Cheat
    {
        public static List<EnemyType> availableEnemyTypes = new List<EnemyType>();
        public static int selectedEnemyTypeIndex = -1;
        public static string spawnAmount = "1";
        public static bool spawnOutside = false;
        
        public override void Trigger()
        {
            // 매 프레임마다 사용 가능한 적 타입들을 업데이트
            UpdateAvailableEnemyTypes();
        }
        
        private void UpdateAvailableEnemyTypes()
        {
            // GameUtil.GetEnemyTypes() 방식으로 적 타입들 가져오기
            availableEnemyTypes.Clear();
            
            if (StartOfRound.Instance == null) return;
            
            foreach (var level in StartOfRound.Instance.levels)
            {
                level.Enemies.ForEach(enemy => { 
                    if (!availableEnemyTypes.Contains(enemy.enemyType)) 
                        availableEnemyTypes.Add(enemy.enemyType); 
                });
                level.DaytimeEnemies.ForEach(enemy => { 
                    if (!availableEnemyTypes.Contains(enemy.enemyType)) 
                        availableEnemyTypes.Add(enemy.enemyType); 
                });
                level.OutsideEnemies.ForEach(enemy => { 
                    if (!availableEnemyTypes.Contains(enemy.enemyType)) 
                        availableEnemyTypes.Add(enemy.enemyType); 
                });
            }
        }
        
        // 적 스폰 메서드 - LethalMenu 방식으로 구현
        public static void SpawnEnemy(EnemyType type, int num, bool outside)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            
            SelectableLevel level = StartOfRound.Instance.currentLevel;
            level.maxEnemyPowerCount = Int32.MaxValue;
            
            GameObject[] gameobject = outside ? RoundManager.Instance.outsideAINodes : RoundManager.Instance.insideAINodes;
            List<Transform> nodes = new List<Transform>();
            
            foreach (GameObject obj in gameobject)
            {
                nodes.Add(obj.transform);
            }
            
            for (int i = 0; i < num; i++)
            {
                Transform node = nodes[UnityEngine.Random.Range(0, nodes.Count)];
                
                if (type.enemyName == "Bush Wolf")
                {
                    // Bush Wolf는 별도 처리 (임시로 일반 스폰 사용)
                    RoundManager.Instance.SpawnEnemyGameObject(node.position, 0.0f, -1, type);
                }
                else
                {
                    RoundManager.Instance.SpawnEnemyGameObject(node.position, 0.0f, -1, type);
                }
            }
            
            // 스폰 완료 메시지
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.DisplayTip("LethalHack", $"Spawned {num} {type.name}!");
            }
        }
        
        // 선택된 적 타입으로 스폰
        public static void SpawnSelectedEnemy()
        {
            if (selectedEnemyTypeIndex < 0 || selectedEnemyTypeIndex >= availableEnemyTypes.Count) return;
            
            EnemyType selectedType = availableEnemyTypes[selectedEnemyTypeIndex];
            int amount = 1;
            
            if (int.TryParse(spawnAmount, out int parsedAmount))
            {
                amount = parsedAmount;
            }
            
            SpawnEnemy(selectedType, amount, spawnOutside);
        }
    }
} 