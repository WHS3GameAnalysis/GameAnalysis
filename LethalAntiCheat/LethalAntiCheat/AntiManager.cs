using GameNetcodeStuff;
using HarmonyLib;
using LethalAntiCheat.AntiCheats;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System;

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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (localPlayer.IsHost)
            {
                Debug.Log("[Start] 호스트 감지, 하모니패치 실행");
                HarmonyPatching();
            }
            else
            {
                Debug.Log("[Start] 호스트 미감지, gameObject Destroy");
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
            harmony = new Harmony("LethalAntiCheat");

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
        }

        //public void Update()
        //
        //    NoClip.Trigger();
        //}

        
    }
}