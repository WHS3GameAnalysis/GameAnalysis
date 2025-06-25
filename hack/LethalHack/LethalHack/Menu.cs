using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LethalHack.Util;

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 350, 250);
        public bool showMenu = true;
        private int currentTab = 0;
        
        // 탭 리스트
        private List<ITab> tabs;
        private string[] tabNames;

        public Menu()
        {
            // 탭 초기화 (새로운 구조)
            tabs = new List<ITab>
            {
                new SelfTab(),      // 플레이어 자신의 능력
                new VisualTab(),    // 시각적 효과,ESP,미니맵
                new EnemyTab(),     // 리스트,소환,죽이기
                new PlayerTab()     // 텔포,데미지,시드입력
            };
            
            // 탭 이름 배열 생성
            tabNames = tabs.Select(tab => tab.TabName).ToArray();
        }

        public void Render()
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack");
            
            // 모든 탭의 별도 창들 렌더링
            foreach (var tab in tabs)
            {
                tab.DrawWindows();
            }
        }

        private void DrawMenu(int windowID)
        {
            // 탭 버튼들 동적 생성 - 균등한 간격으로 배치
            float totalWidth = windowRect.width - 20; // 좌우 여백 10씩
            float buttonWidth = 75; // 버튼 너비를 약간 줄임
            float buttonHeight = 30;
            float totalButtonWidth = tabs.Count * buttonWidth;
            float spacing = (totalWidth - totalButtonWidth) / (tabs.Count - 1); // 탭 사이 간격
            
            for (int i = 0; i < tabs.Count; i++)
            {
                float xPos = 10 + (i * (buttonWidth + spacing));
                
                if (GUI.Button(new Rect(xPos, 35, buttonWidth, buttonHeight), tabNames[i]))
                {
                    currentTab = i;
                }
            }

            // 현재 선택된 탭 그리기
            if (currentTab >= 0 && currentTab < tabs.Count)
            {
                tabs[currentTab].DrawTab();
            }

            GUI.DragWindow();
        }
    }
}