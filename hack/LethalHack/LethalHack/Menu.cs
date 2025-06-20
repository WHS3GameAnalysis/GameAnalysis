﻿using UnityEngine;

namespace LethalHack
{
    // GUI를 띄우는 역할
    public class Menu : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 200, 120); // GUI 창 위치, 크기 지정
        public bool showMenu = true; // 창 표시 여부
        
        public void Render() // 이 메서드는 Loader에서 호출되어서 GUI를 그리는 역할을 합니다.
        {
            if (!showMenu) return;

            windowRect = GUI.Window(1, windowRect, DrawMenu, "LethalHack"); // GUI 창 생성: ID = 1, 위치 = windowRect, 내용 = DrawMenu 함수
        }

        private void DrawMenu(int windowID) // GUI 창 안에 들어갈 내용
        {
            // God Mode 토글 버튼. 누르면 Hack.Instance.God의 isEnabled 값을 true/false로 토글
            Hack.Instance.God.isEnabled = GUI.Toggle(new Rect(10, 20, 180, 20), Hack.Instance.God.isEnabled, "God Mode");
            // 위와 마찬가지
            Hack.Instance.Stamina.isEnabled = GUI.Toggle(new Rect(10, 45, 180, 20), Hack.Instance.Stamina.isEnabled, "Infinite Stamina");

            Hack.Instance.Hpdisp.isEnabled = GUI.Toggle(new Rect(10, 70, 180, 20), Hack.Instance.Hpdisp.isEnabled, "HPDisplay");

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }
    }
}