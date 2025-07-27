
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Anti_Cheat.AntiCheats;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System;
using Lethal_Anti_Cheat.Core;
using Steamworks;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat
{
    public static class AntiManager
    {
        public static void KickPlayer(PlayerControllerB player, string reason)
        {
            PipeLogger.Log($"[Behaviour] LethalAntiCheat: Kicking player {player.playerUsername} for: {reason}");

            if (player.playerSteamId != 0)
            {
                StartOfRound.Instance.KickedClientIds.Add(player.playerSteamId);
            }

            NetworkManager.Singleton.DisconnectClient(player.playerClientId);

            PipeLogger.Log($"[Behaviour][LethalAntiCheat] Kicking player {player.playerUsername} for: {reason}");
        }
    }
}
