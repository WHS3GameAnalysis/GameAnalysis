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
    public abstract class Cheat
    {
        public bool isEnabled = false;
        public abstract void Trigger();
    }

    public class Hack : MonoBehaviour
    {
        public static Hack Instance;

        // Cheat 기능들 선언
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        internal HPDisplay Hpdisp = new HPDisplay();
        public Freecam freecam = new Freecam();

        public static PlayerControllerB localPlayer;
        public static Harmony harmony;


        Menu GUIManager = new Menu();

        public void Start()
        {
            Instance = this;
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer == null) return;

            HarmonyPatching();
        }

        private void HarmonyPatching()
        {
            harmony = new Harmony("LethalHack");
            Harmony.DEBUG = false;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
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
            if (freecam.isEnabled) freecam.Trigger();
            // if (SuperJump.isEnabled) SuperJump.Trigger();

        }
    }
}