using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LethalHack.Cheats; // EnemyList 클래스를 사용하기 위해 추가

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 350, 200); // GUI 창 크기를 늘림
        public bool showMenu = true; // 창 표시 여부
        private int currentTab = 0; // 선택된 탭 (0: Player, 1: Enemy, 2: Visual)
        private Vector2 enemyListScrollPos = Vector2.zero; // Enemy List 스크롤 위치
        
        // 별도 창 관련 변수들
        private bool showEnemyListWindow = false;
        private bool showEnemySpawnWindow = false;
        private Rect enemyListWindowRect = new Rect(50, 50, 250, 400); // 가로 크기를 500에서 250으로 줄임
        private Rect enemySpawnWindowRect = new Rect(100, 100, 400, 300);
        
        // 드롭다운 관련 변수들
        private bool showEnemyTypeDropdown = false;
        private Vector2 enemyTypeScrollPos = Vector2.zero;
        
        public void Render() // 이 메서드는 Loader에서 호출되어서 GUI를 그리는 역할을 합니다.
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack"); // GUI 창 생성: ID = 1, 위치 = windowRect, 내용 = DrawMenu 함수
            
            // 별도 창들 렌더링
            if (showEnemyListWindow)
            {
                enemyListWindowRect = GUI.Window(2, enemyListWindowRect, DrawEnemyListWindow, "Enemy List");
            }
            
            if (showEnemySpawnWindow)
            {
                enemySpawnWindowRect = GUI.Window(3, enemySpawnWindowRect, DrawEnemySpawnWindow, "Enemy Spawn");
            }
        }

        private void DrawMenu(int windowID) // GUI 창 안에 들어갈 내용
        {
            // 탭 버튼들
            if (GUI.Button(new Rect(10, 35, 80, 30), "Player")) currentTab = 0;
            if (GUI.Button(new Rect(95, 35, 80, 30), "Enemy")) currentTab = 1;
            if (GUI.Button(new Rect(180, 35, 80, 30), "Visual")) currentTab = 2;

            // 탭별 내용
            switch (currentTab)
            {
                case 0: // Player 탭
                    DrawPlayerTab();
                    break;
                case 1: // Enemy 탭
                    DrawEnemyTab();
                    break;
                case 2: // Visual 탭
                    DrawVisualTab();
                    break;
            }

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }

        private void DrawPlayerTab()
        {
            // 기존 플레이어 관련 토글들
            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, 80, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, 105, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");
            Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, 130, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HPDisplay");
            Hack.Instance.SuperJump.isEnabled = GUI.Toggle(new Rect(10, 155, 180, 20), Hack.Instance.SuperJump.isEnabled, "Super Jump");
        }

        private void DrawEnemyTab()
        {
            // 체크박스들
            showEnemyListWindow = GUI.Toggle(new Rect(10, 80, 150, 20), showEnemyListWindow, "Enemy List");
            showEnemySpawnWindow = GUI.Toggle(new Rect(10, 105, 150, 20), showEnemySpawnWindow, "Enemy Spawn");
            
            // 퀵 액션 버튼
            if (GUI.Button(new Rect(10, 135, 120, 25), "Kill All")) 
            {
                // Kill All 기능 실행
                EnemyList.KillAllEnemies();
            }
        }
        
        private void DrawEnemyListWindow(int windowID)
        {
            // 창 닫기 버튼
            if (GUI.Button(new Rect(220, 5, 20, 20), "X")) // 위치 조정
            {
                showEnemyListWindow = false;
            }
            
            // Enemy List 제목 (총 개수 포함)
            GUI.Label(new Rect(10, 30, 230, 20), $"Enemy List ({EnemyList.enemies.Count})"); // 너비 조정
            
            // Enemy List 스크롤 영역
            enemyListScrollPos = GUI.BeginScrollView(new Rect(10, 55, 230, 320), enemyListScrollPos, new Rect(0, 0, 210, 400)); // 크기 조정
            
            // EnemyList.enemies를 사용하여 적 목록 표시
            if (Hack.localPlayer != null)
            {
                if (EnemyList.enemies.Count == 0)
                {
                    GUI.Label(new Rect(0, 0, 210, 20), "No enemies found"); // 너비 조정
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
                            
                            GUI.Label(new Rect(0, i * 20, 210, 20), enemyInfo); // 너비 조정
                        }
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(0, 0, 210, 20), "Player not found"); // 너비 조정
            }
            
            GUI.EndScrollView();
            
            GUI.DragWindow(); // 창 드래그 가능
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
            
            GUI.DragWindow(); // 창 드래그 가능
        }
        
        private void DrawVisualTab()
        {
            // NoVisor 토글만 남김
            Hack.Instance.NoVisorHack.isEnabled = GUI.Toggle(new Rect(10, 80, 180, 20), Hack.Instance.NoVisorHack.isEnabled, "NoVisor (헬멧 없애기)");
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