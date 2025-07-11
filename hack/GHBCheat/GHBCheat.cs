﻿using GameNetcodeStuff;
using GHBCheat.Menu.Core;
using GHBCheat.Themes;
using GHBCheat.Util;
using HarmonyLib;
using GHBCheat.Language;
//using GHBCheat.Manager;
using GHBCheat.Menu.Popup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace GHBCheat
{
    public class GHBCheat : MonoBehaviour
    {
        private List<Cheat> cheats;
        //public static List<GrabbableObject> items = new List<GrabbableObject>();
        //public static List<Landmine> landmines = new List<Landmine>();
        //public static List<Turret> turrets = new List<Turret>();
        //public static List<EntranceTeleport> doors = new List<EntranceTeleport>();
        //public static List<PlayerControllerB> players = new List<PlayerControllerB>();
        //public static List<EnemyAI> enemies = new List<EnemyAI>();
        //public static List<SteamValveHazard> steamValves = new List<SteamValveHazard>();
        //public static List<TerminalAccessibleObject> bigDoors = new List<TerminalAccessibleObject>();
        //public static List<TerminalAccessibleObject> allTerminalObjects = new List<TerminalAccessibleObject>();
        //public static List<DoorLock> doorLocks = new List<DoorLock>();
        //public static List<ShipTeleporter> teleporters = new List<ShipTeleporter>();
        //public static List<InteractTrigger> interactTriggers = new List<InteractTrigger>();
        //public static List<SpikeRoofTrap> spikeRoofTraps = new List<SpikeRoofTrap>();
        //public static List<GameObject> vainShrouds = new List<GameObject>();
        //public static List<AnimatedObjectTrigger> animatedTriggers = new List<AnimatedObjectTrigger>();
        //public static List<EnemyVent> enemyVents = new List<EnemyVent>();
        //public static List<VehicleController> vehicles = new List<VehicleController>();
        //public static List<LocalVolumetricFog> fogs = new List<LocalVolumetricFog>();
        //public static List<Volume> volumes = new List<Volume>();
        //public static ItemDropship itemDropship;
        //public static HangarShipDoor shipDoor;
        //public static DepositItemsDesk depositItemsDesk;
        //public static BreakerBox breaker;
        //public static MineshaftElevatorController mineshaftElevator;
        public static PlayerControllerB localPlayer;
        //public static QuickMenuManager quickMenuManager;
        //public static Terminal terminal;
        //public static int selectedPlayer = -1;
        public int fps;
        //public Dictionary<string, string> LMUsers = [];

        public static Harmony harmony;
        private HackMenu menu;
        private static GHBCheat instance;
        public static GHBCheat Instance
        {
            get
            {
                if (instance == null)
                    instance = new GHBCheat();
                return instance;
            }
        }

        public void Start()
        {
            instance = this;
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Settings.debugMessage = (e.Message + "\n" + e.StackTrace);
            }
        }

        private void Initialize()
        {
            Localization.Initialize();
            Theme.Initialize();
            HarmonyPatching();
            LoadCheats();
            //MenuUtil.StartLMUser();
            //ObjectManager.CollectObjects();
            this.StartCoroutine(this.FPSCounter());
        }

        private void HarmonyPatching()
        {
            harmony = new Harmony("GHBCheat");
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

        private void LoadCheats()
        {
            try
            {
                Settings.Changelog.ReadChanges();
                cheats = new List<Cheat>();
                menu = new HackMenu();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "GHBCheat.Cheats", StringComparison.Ordinal) && t.IsSubclassOf(typeof(Cheat)))) cheats.Add((Cheat)Activator.CreateInstance(type));
                Settings.Config.SaveDefaultConfig();
                Settings.Config.LoadConfig();
            }
            catch (Exception e)
            {
                Settings.debugMessage = (e.Message);
            }
        }

        public void FixedUpdate()
        {
            try
            {
                if ((bool)StartOfRound.Instance) cheats.ForEach(cheat => cheat.FixedUpdate());
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }

        public void Update()
        {
            if (!(bool)StartOfRound.Instance || StartOfRound.Instance.inShipPhase) Settings.v_savedLocation = Vector3.zero;
            try
            {
                foreach (Hack hack in Enum.GetValues(typeof(Hack)))
                {
                    if ((bool)StartOfRound.Instance && localPlayer != null && (localPlayer.isTypingChat || localPlayer.quickMenuManager.isMenuOpen || localPlayer.inTerminalMenu)) continue;
                    if (hack.HasKeyBind() && hack.GetKeyBind().wasPressedThisFrame && !hack.IsAnyHackWaiting()) hack.Execute();
                    if (!(bool)StartOfRound.Instance) return;
                }
                cheats.ForEach(cheat => cheat.Update());
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }

        public IEnumerator FPSCounter()
        {
            while (true)
            {
                fps = (int)(1.0f / Time.deltaTime);
                yield return new WaitForSeconds(1f);
            }
        }

        public void OnGUI()
        {
            try
            {
                if (Event.current.type == EventType.Repaint)
                {
                    string GHBCheatTitle = $"GHBCheat {Settings.version} 게임 해체 분석기. | Menu Toggle: {FirstSetupManagerWindow.GetMenuKeybindName()}";
                    GHBCheatTitle += Settings.b_FPSCounter ? $" | FPS: {fps}" : "";
                    VisualUtil.DrawString(new Vector2(5f, 2f), GHBCheatTitle, Settings.c_primary, centered: false, bold: true, fontSize: 14);
                    if (MenuUtil.resizing)
                    {
                        VisualUtil.DrawString(
                            new Vector2(Screen.width / 2, 35f),
                            Localization.Localize(
                                new string[] { "SettingsTab.ResizeTitle", "SettingsTab.ResizeConfirm", $"{HackMenu.Instance.windowRect.width}x{HackMenu.Instance.windowRect.height}" },
                                true
                            ),
                            Settings.c_playerESP, true, true, true, true, 22
                        );
                        MenuUtil.ResizeMenu();
                    }
                    if (Settings.DebugMode)
                    {
                        VisualUtil.DrawString(new Vector2(5f, 20f), "[DEBUG MODE]", new RGBAColor(50, 205, 50, 1f), false, false, false, true, 10);
                        VisualUtil.DrawString(new Vector2(10f, 65f), new RGBAColor(255, 195, 0, 1f).AsString(Settings.debugMessage), false, false, false, false, 22);
                    }
                    if ((bool)StartOfRound.Instance) cheats.ForEach(cheat => cheat.OnGui());
                }
                menu.Draw();
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }

        public void Unload()
        {
            this.StopAllCoroutines();
            Loader.Unload();
        }
    }
}