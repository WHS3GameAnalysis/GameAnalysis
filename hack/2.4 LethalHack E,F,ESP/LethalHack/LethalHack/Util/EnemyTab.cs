using UnityEngine;
using LethalHack.Cheats;
using System.Collections.Generic;

namespace LethalHack.Util
{
    public class EnemyTab : ITab
    {
        public string TabName => "Enemy";

        // 별도 창 관련 변수들
        private bool showEnemyListWindow = false;
        private bool showEnemySpawnWindow = false;
        private Rect enemyListWindowRect = new Rect(50, 50, 250, 400);
        private Rect enemySpawnWindowRect = new Rect(100, 100, 400, 300);
        
        // 드롭다운 관련 변수들
        private bool showEnemyTypeDropdown = false;
        private Vector2 enemyTypeScrollPos = Vector2.zero;
        private Vector2 enemyListScrollPos = Vector2.zero;

        public void DrawTab()
        {
            // 체크박스들
            showEnemyListWindow = GUI.Toggle(new Rect(10, 80, 150, 20), showEnemyListWindow, "Enemy List");
            showEnemySpawnWindow = GUI.Toggle(new Rect(10, 105, 150, 20), showEnemySpawnWindow, "Enemy Spawn");
            
            // 퀵 액션 버튼
            if (GUI.Button(new Rect(10, 135, 120, 25), "Kill All")) 
            {
                EnemyList.KillAllEnemies();
            }
        }

        public void DrawWindows()
        {
            if (showEnemyListWindow)
            {
                enemyListWindowRect = GUI.Window(2, enemyListWindowRect, DrawEnemyListWindow, "Enemy List");
            }
            
            if (showEnemySpawnWindow)
            {
                enemySpawnWindowRect = GUI.Window(3, enemySpawnWindowRect, DrawEnemySpawnWindow, "Enemy Spawn");
            }
        }

        private void DrawEnemyListWindow(int windowID)
        {
            // 창 닫기 버튼
            if (GUI.Button(new Rect(220, 5, 20, 20), "X"))
            {
                showEnemyListWindow = false;
            }
            
            // Enemy List 제목 (총 개수 포함)
            GUI.Label(new Rect(10, 30, 230, 20), $"Enemy List ({EnemyList.enemies.Count})");
            
            // Enemy List 스크롤 영역
            enemyListScrollPos = GUI.BeginScrollView(new Rect(10, 55, 230, 320), enemyListScrollPos, new Rect(0, 0, 210, 400));
            
            // EnemyList.enemies를 사용하여 적 목록 표시
            if (Hack.localPlayer != null)
            {
                if (EnemyList.enemies.Count == 0)
                {
                    GUI.Label(new Rect(0, 0, 210, 20), "No enemies found");
                }
                else
                {
                    for (int i = 0; i < EnemyList.enemies.Count; i++)
                    {
                        if (EnemyList.enemies[i] != null)
                        {
                            string enemyName = EnemyList.enemies[i].enemyType != null ? EnemyList.enemies[i].enemyType.name : "Unknown Enemy";
                            float distance = GetDistanceToPlayer(EnemyList.enemies[i].transform.position);
                            string enemyInfo = $"{enemyName} ({distance}m)";
                            
                            GUI.Label(new Rect(0, i * 20, 210, 20), enemyInfo);
                        }
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(0, 0, 210, 20), "Player not found");
            }
            
            GUI.EndScrollView();
            GUI.DragWindow();
        }
        
        private void DrawEnemySpawnWindow(int windowID)
        {
            // 창 닫기 버튼
            if (GUI.Button(new Rect(370, 5, 20, 20), "X"))
            {
                showEnemySpawnWindow = false;
            }
            
            GUI.Label(new Rect(10, 30, 380, 20), "Enemy Spawn");
            
            // 호스트 체크
            if (Hack.localPlayer == null || !Hack.localPlayer.IsHost)
            {
                GUI.Label(new Rect(10, 60, 380, 20), "Host required to spawn enemies!");
                return;
            }
            
            // Enemy Type 선택 (드롭다운)
            GUI.Label(new Rect(10, 60, 100, 20), "Enemy Type:");
            
            if (LethalHack.Cheats.EnemySpawn.availableEnemyTypes.Count == 0)
            {
                GUI.Label(new Rect(120, 60, 200, 20), "No enemy types available");
            }
            else
            {
                // 선택된 적 타입 이름 가져오기
                string selectedEnemyTypeName = "Select Enemy Type";
                if (LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex >= 0 && 
                    LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex < LethalHack.Cheats.EnemySpawn.availableEnemyTypes.Count)
                {
                    selectedEnemyTypeName = LethalHack.Cheats.EnemySpawn.availableEnemyTypes[LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex].name;
                }
                
                // 드롭다운 버튼
                if (GUI.Button(new Rect(120, 60, 200, 20), selectedEnemyTypeName))
                {
                    showEnemyTypeDropdown = !showEnemyTypeDropdown;
                }
                
                // 드롭다운 목록 (조건부 표시)
                if (showEnemyTypeDropdown)
                {
                    // 드롭다운 배경 박스
                    GUI.Box(new Rect(120, 80, 200, 150), "");
                    
                    // 스크롤 영역
                    enemyTypeScrollPos = GUI.BeginScrollView(new Rect(120, 80, 200, 150), enemyTypeScrollPos, new Rect(0, 0, 180, LethalHack.Cheats.EnemySpawn.availableEnemyTypes.Count * 25));
                    
                    for (int i = 0; i < LethalHack.Cheats.EnemySpawn.availableEnemyTypes.Count; i++)
                    {
                        // 선택된 항목 하이라이트
                        if (i == LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex)
                        {
                            GUI.backgroundColor = Color.cyan;
                        }
                        
                        if (GUI.Button(new Rect(0, i * 25, 180, 20), LethalHack.Cheats.EnemySpawn.availableEnemyTypes[i].name))
                        {
                            LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex = i;
                            showEnemyTypeDropdown = false; // 선택 후 드롭다운 닫기
                        }
                        
                        // 배경색 원래대로
                        GUI.backgroundColor = Color.white;
                    }
                    
                    GUI.EndScrollView();
                }
            }
            
            // Amount 입력
            GUI.Label(new Rect(10, 90, 100, 20), "Amount:");
            LethalHack.Cheats.EnemySpawn.spawnAmount = GUI.TextField(new Rect(120, 90, 50, 20), LethalHack.Cheats.EnemySpawn.spawnAmount);
            
            // 숫자만 입력되도록 필터링
            if (!int.TryParse(LethalHack.Cheats.EnemySpawn.spawnAmount, out int _))
            {
                LethalHack.Cheats.EnemySpawn.spawnAmount = "1";
            }
            
            // Spawn Outside 체크박스
            LethalHack.Cheats.EnemySpawn.spawnOutside = GUI.Toggle(new Rect(10, 120, 150, 20), LethalHack.Cheats.EnemySpawn.spawnOutside, "Spawn Outside");
            
            // Spawn 버튼
            if (GUI.Button(new Rect(10, 150, 100, 25), "Spawn Enemy"))
            {
                if (LethalHack.Cheats.EnemySpawn.selectedEnemyTypeIndex >= 0)
                {
                    LethalHack.Cheats.EnemySpawn.SpawnSelectedEnemy();
                }
                else
                {
                    if (HUDManager.Instance != null)
                    {
                        HUDManager.Instance.DisplayTip("LethalHack", "Please select an enemy type!");
                    }
                }
            }
            
            GUI.DragWindow();
        }
        
        // 플레이어와 적 사이의 거리 계산
        private float GetDistanceToPlayer(Vector3 enemyPosition)
        {
            if (Hack.localPlayer != null)
            {
                return Mathf.Round(Vector3.Distance(Hack.localPlayer.transform.position, enemyPosition));
            }
            return 0f;
        }
    }
} 