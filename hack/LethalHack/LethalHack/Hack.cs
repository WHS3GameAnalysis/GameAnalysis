using GameNetcodeStuff;
using HarmonyLib;
using LethalHack.Cheats;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
//using System.Diagnostics;

namespace LethalHack
{
    public abstract class Cheat // 세부 기능을 구현할 때 사용할 추상 클래스
    {
        public bool isEnabled = true; // On/Off
        public abstract void Trigger(); // Trigger 메서드로 기능 실행
    }

    public class Hack : MonoBehaviour // 기능을 실행하기 위해 사용되는 클래스
    {
        public static Hack Instance = new Hack(); // 싱글톤으로 외부에서도 참조 가능하게 만들었습니다.

        // 기능 인스턴스들
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        public HPDisplay Hpdisp = new HPDisplay();
        internal SuperJump SuperJump = new SuperJump();
        internal NoClip NoClip = new NoClip(); // ✅ NoClip 추가

        Menu GUIManager = new Menu(); // GUI를 띄우기 위해서 Menu 객체를 하나 만들어줍니다.
        public static PlayerControllerB localPlayer;
        public static Harmony harmony; // Harmony 인스턴스

        public void Start()
        {
            if (Instance == null) Instance = this; // 싱글톤 인스턴스 설정
            localPlayer = GameNetworkManager.Instance.localPlayerController; // 로컬 플레이어 초기화
            if (localPlayer == null) return; // 로컬 플레이어가 없으면 종료
            HarmonyPatching(); // Harmony 패치 적용
        }

        private void HarmonyPatching()
        {
            harmony = new Harmony("LethalHack");
            Harmony.DEBUG = false;
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsDefined(typeof(HarmonyPatch), false))
                {
                    try
                    {
                        new PatchClassProcessor(harmony, type).Patch();
                    }
                    catch
                    {
                        Debug.LogWarning($"Skipping patch in {type.FullName}");
                    }
                }
            }
        }

        public void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 80), "Cheat Enabled");
            GUIManager.Render();
        }

        public void Update()
        {
            if (God.isEnabled) God.Trigger();
            if (Stamina.isEnabled) Stamina.Trigger();
            if (Hpdisp.isEnabled) Hpdisp.Trigger();
            if (NoClip.isEnabled) NoClip.Trigger();
            //if (SuperJump.isEnabled) SuperJump.Trigger();
        }
    }
}
