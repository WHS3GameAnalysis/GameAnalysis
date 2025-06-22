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

        // 기능의 인스턴스를 만들고,
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        internal HPDisplay Hpdisp = new HPDisplay();
        internal SuperJump SuperJump = new SuperJump();
        public Teleport teleport = new Teleport();
        public DamageHack damageHack = new DamageHack();
        public Minimap minimap = new Minimap();

        Menu GUIManager = new Menu(); // GUI를 띄우기 위해서 Menu 객체를 하나 만들어줍니다.
        public static PlayerControllerB localPlayer;
        public static Harmony harmony; // Harmony 인스턴스

        // Start 메서드에서 On/Off 여부에 따라 기능을 실행합니다.
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

        public void OnGUI() // Update 메서드 이후에 호출되는 메서드
        {
            GUI.Label(new Rect(10, 10, 400, 80), "Cheat Enabled"); // 왼쪽 상단 위에 표시할 문구
            GUIManager.Render(); // GUI 렌더링
        }

        public void Update() // Unity에서 매 프레임마다 호출되는 메서드
        {
            if (God.isEnabled) God.Trigger();
            if (Stamina.isEnabled) Stamina.Trigger();
            if (Hpdisp.isEnabled) Hpdisp.Trigger();
            //if (SuperJump.isEnabled) SuperJump.Trigger();
            //if (!SuperJump.isEnabled) SuperJump.Trigger();
        }
    }
}