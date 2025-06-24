using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LethalHack.Cheats; // EnemyList 클래스를 사용하기 위해 추가

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 300, 200); // GUI 창 크기를 늘림
        public bool showMenu = true; // 창 표시 여부
        private int selectedTab = 0; // 선택된 탭 (0: Player, 1: Enemy)
        private Vector2 enemyListScrollPos = Vector2.zero; // Enemy List 스크롤 위치
        
        public void Render() // 이 메서드는 Loader에서 호출되어서 GUI를 그리는 역할을 합니다.
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack"); // GUI 창 생성: ID = 1, 위치 = windowRect, 내용 = DrawMenu 함수
        }

        private void DrawMenu(int windowID) // GUI 창 안에 들어갈 내용
        {
            // 탭 버튼들
            if (GUI.Button(new Rect(10, 25, 60, 20), "Player"))
            {
                selectedTab = 0;
            }
            if (GUI.Button(new Rect(75, 25, 60, 20), "Enemy"))
            {
                selectedTab = 1;
            }

            // 탭별 내용
            switch (selectedTab)
            {
                case 0: // Player 탭
                    DrawPlayerTab();
                    break;
                case 1: // Enemy 탭
                    DrawEnemyTab();
                    break;
            }

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }

        private void DrawPlayerTab()
        {
            // 기존 플레이어 관련 토글들
            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, 55, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, 80, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");
            Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, 105, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HPDisplay");
            Hack.Instance.SuperJump.isEnabled = GUI.Toggle(new Rect(10, 130, 180, 20), Hack.Instance.SuperJump.isEnabled, "Super Jump");
        }

        private void DrawEnemyTab()
        {
            // Enemy List 제목
            GUI.Label(new Rect(10, 55, 280, 20), "Enemy List:");

            // Enemy List 스크롤 영역
            enemyListScrollPos = GUI.BeginScrollView(new Rect(10, 75, 280, 100), enemyListScrollPos, new Rect(0, 0, 260, 200));

            // EnemyList.enemies를 사용하여 적 목록 표시
            if (Hack.localPlayer != null)
            {
                if (EnemyList.enemies.Count == 0)
                {
                    GUI.Label(new Rect(0, 0, 260, 20), "No enemies found");
                }
                else
                {
                    for (int i = 0; i < EnemyList.enemies.Count; i++)
                    {
                        if (EnemyList.enemies[i] != null)
                        {
                            string enemyName = EnemyList.enemies[i].enemyType != null ? EnemyList.enemies[i].enemyType.name : "Unknown Enemy";
                            string enemyStatus = EnemyList.enemies[i].isEnemyDead ? " (Dead)" : " (Alive)";
                            string enemyInfo = $"{enemyName}{enemyStatus}";
                            
                            GUI.Label(new Rect(0, i * 20, 260, 20), enemyInfo);
                        }
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(0, 0, 260, 20), "Player not found");
            }

            GUI.EndScrollView();
        }
    }
}