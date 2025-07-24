using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.AntiCheats;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System;
using LethalAntiCheat.Core;
using Steamworks;

namespace LethalAntiCheat
{
    // public abstract class AntiBase
    // {
    //     public bool isUsingHack = false;
    //     public abstract void Trigger();
        
    // }
    public class AntiManager : MonoBehaviour
    {
        
        //핵 탐지 클래스 인스턴스
        public static AntiManager Instance = new AntiManager();
        public static Harmony harmony;
        public static PlayerControllerB localPlayer;
        private bool isInitialized = false;

        //public GodMode God = new GodMode();
        //public InfinityStamina Stamina = new InfinityStamina();
        //public HPDisplay Hpdisp = new HPDisplay();
        //internal SuperJump SuperJump = new SuperJump();
        //public FastClimb FastClimbHack = new FastClimb();
        //public ESP ESPHack = new ESP();
        //public DamageHack DamageHack = new DamageHack();
        //public Teleport TeleportHack = new Teleport();
        //public Minimap MinimapHack = new Minimap();
        //public InputSeed InputSeedHack = new InputSeed();
        //public EnemyList EnemyListHack = new EnemyList();
        //public EnemySpawn EnemySpawnHack = new EnemySpawn();
        //public NoVisor NoVisorHack = new NoVisor();


        public void Start()
        {
            Instance = this;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            if (isInitialized) return;

            if (StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null)
            {
                localPlayer = StartOfRound.Instance.localPlayerController;
                Initialize();
                isInitialized = true;
            }
        }

        private void Initialize()
        {
            if (localPlayer.IsHost)
            {
                Core.MessageUtils.ShowMessage("[LethalAntiCheat] LethalAntiCheat Loading...");
                HarmonyPatching();
                Core.PatchDetector.Start(); // PatchDetector 작동
                AntiCheats.HarmonyPatchDetect.ReceivingIllegalPatch(harmony); // HarmonyPatchDetect에 Harmony 인스턴스를 전달
            }
            else
            {
                Core.MessageUtils.ShowMessage("[LethalAntiCheat] Host Undetected, Shutting down...");
                Destroy(gameObject);
            }
        }

        private bool IsHost()
        {
            //return StartOfRound.Instance != null && StartOfRound.Instance.isHostPlayerObject; 임시로 주석처리. 수정하지 말 것.
            return true;
        }

        private void HarmonyPatching()
        {
           // Core.MessageUtils.ShowMessage("[DEBUG] HarmonyPatching Started!");
            harmony = new Harmony("LethalAntiCheat");
            //harmony.Patch(AccessTools.Method(typeof(NetworkManager), "Awake"),
            //              prefix: new HarmonyMethod(typeof(AntiCheatUtils), nameof(NetworkManagerAwakePrefix)));

            //ApplyCorePatches(harmony);
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
            Debug.Log("[harmonyPatching] 하모니 패치 완료");
            Core.MessageUtils.ShowMessage("[LethalAntiCheat] LethalAntiCheat Ready");
        }

        //핵 탐지 클래스를 개별로 추가해서 어셈블리 목록 줘야할 시 사용. 현재는 전체 어셈블리 구조를 가져와서 패치함. (HarmonyPatching)
        // private void ApplyCorePatches(Harmony harmonyInstance)
        // {
        //     
        //     Core.AntiCheatUtils.PatchTransport(harmonyInstance);
        //     harmonyInstance.PatchAll(typeof(GrabObjectPatch).Assembly);
        //     harmonyInstance.PatchAll(typeof(NoClip).Assembly);
        // }
        public void KickPlayer(PlayerControllerB player, string reason)
        {

            Debug.Log($"LethalAntiCheat: Kicking player {player.playerUsername} for: {reason}");

            if (player.playerSteamId != 0)
            {
                StartOfRound.Instance.KickedClientIds.Add(player.playerSteamId);
            }

            NetworkManager.Singleton.DisconnectClient(player.playerClientId);

            Core.MessageUtils.ShowMessage($"[LethalAntiCheat] Kicking player {player.playerUsername} for: {reason}");
            //Core.MessageUtils.ShowHostOnlyMessage($"[LethalAntiCheat] Kicking player {player.playerUsername} for: {reason}");
        }

        //[HarmonyPatch(typeof(StartOfRound), "Start")]
        //public static class RoundStartPatch
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //        {
        //            Instance.ResetState();
        //        }
        //}

        //public void ResetState()
        //{
        //    Debug.Log("[LethalAntiCheat] Resetting Anti-Cheat state for new session...");

        //    // Clear static collections in anti-cheat modules
        //    AntiCheats.DamageHack.warnedPlayers.Clear();
        //    AntiCheats.SuperJumpAndFastClimb.climbTracker.Clear();
        //    AntiCheats.SuperJumpAndFastClimb.jumpTracker.Clear();

        //    // Clear static collections in Core
        //    Core.AntiCheatUtils.ConnectionIdToSteamIdMap.Clear();
        //    Core.PatchDetector.ClearCompromisedMethods(); 

        //    // Reset initialization flag
        //    isInitialized = false;

        //    // Stop and restart PatchDetector timer if it's running
        //    Core.PatchDetector.Stop();
        //    Core.PatchDetector.Start();

        //    Core.MessageUtils.ShowMessage("[LethalAntiCheat] Anti-Cheat state reset complete.");
        //}
    }
}