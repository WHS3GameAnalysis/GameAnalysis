
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Lethal_Anti_Cheat.Util;
using System;

namespace Lethal_Anti_Cheat.Core
{
    public static class AntiCheatUtils
    {
        public static Dictionary<uint, ulong> ConnectionIdToSteamIdMap { get; } = new Dictionary<uint, ulong>();

        public static bool CheckAndGetUser(ulong senderClientId, out PlayerControllerB player)
        {
            player = null;

            if (senderClientId == NetworkManager.Singleton.LocalClientId)
            {
                return false;
            }

            player = GetPlayerByClientId(senderClientId);

            if (player == null)
            {
                AntiManager.KickPlayer(new PlayerControllerB() { playerClientId = senderClientId }, "Invalid player object.");
                return false;
            }

            if (StartOfRound.Instance.KickedClientIds.Contains(player.playerSteamId))
            {
                return false;
            }

            if (player.playerSteamId == 0)
            {
                uint transportId = ClientIdToTransportId(senderClientId);
                if (transportId != 0 && ConnectionIdToSteamIdMap.TryGetValue(transportId, out ulong steamId))
                {
                    Friend f = new Friend(steamId);
                    //Debug.Log($"LethalAntiCheat: Player {f.Name}({steamId}) has an invalid SteamID and will be kicked.");
                    PipeLogger.Log($"[Behaviour] LethalAntiCheat: Player {f.Name}({steamId}) has an invalid SteamID and will be kicked.");
                    //AntiManager.KickPlayer(player, "Invalid SteamID");
                }
                return false;
            }

            return true;
        }

        private static PlayerControllerB GetPlayerByClientId(ulong clientId)
        {
            if (StartOfRound.Instance == null) return null;
            return StartOfRound.Instance.allPlayerScripts.FirstOrDefault(p => p.isPlayerControlled && p.actualClientId == clientId);
        }

        private static uint ClientIdToTransportId(ulong clientId)
        {
            if (NetworkManager.Singleton == null) return 0;

            var networkConnectionManager = Traverse.Create(NetworkManager.Singleton).Field("ConnectionManager").GetValue<NetworkConnectionManager>();
            var transportId = Traverse.Create(networkConnectionManager).Method("ClientIdToTransportId", new object[] { clientId }).GetValue<ulong>();
            return (uint)transportId;
        }

        public static void PatchTransport(Harmony harmony)
        {
            try
            {
                var onConnectingMethod = AccessTools.Method(typeof(FacepunchTransport), "Steamworks.ISocketManager.OnConnecting");
                var onDisconnectedMethod = AccessTools.Method(typeof(FacepunchTransport), "Steamworks.ISocketManager.OnDisconnected");

                if (onConnectingMethod != null)
                {
                    harmony.Patch(onConnectingMethod, prefix: new HarmonyMethod(typeof(AntiCheatUtils), nameof(OnConnectingPrefix)));
                }
                else
                {
                    //Debug.LogError("LethalAntiCheat: Failed to find FacepunchTransport.OnConnecting method.");
                    PipeLogger.Log("[Behaviour] LethalAntiCheat: Failed to find FacepunchTransport.OnConnecting method.");
                    //Console.WriteLine("LethalAntiCheat: Failed to find FacepunchTransport.OnConnecting method.");
                }

                if (onDisconnectedMethod != null)
                {
                    harmony.Patch(onDisconnectedMethod, prefix: new HarmonyMethod(typeof(AntiCheatUtils), nameof(OnDisconnectedPrefix)));
                }
                else
                {
                    //Debug.LogError("LethalAntiCheat: Failed to find FacepunchTransport.OnDisconnected method.");
                    PipeLogger.Log("[Behaviour] LethalAntiCheat: Failed to find FacepunchTransport.OnDisconnected method.");
                    //Console.WriteLine("LethalAntiCheat: Failed to find FacepunchTransport.OnDisconnected method.");
                }
            }
            catch (NetworkConfigurationException ex)
            {
                //Debug.LogError($"LethalAntiCheat: Exception while patching transport: {ex}");
                PipeLogger.Log($"[Behaviour] LethalAntiCheat: Exception while patching transport: {ex}");
                //Console.WriteLine($"LethalAntiCheat: Exception while patching transport: {ex}");
            }
        }

        private static bool OnConnectingPrefix(ref Connection connection, ref ConnectionInfo info)
        {
            NetIdentity identity = Traverse.Create(info).Field<NetIdentity>("identity").Value;
            ulong steamId = identity.SteamId.Value;

            if (StartOfRound.Instance.KickedClientIds.Contains(steamId))
            {
                //Debug.Log($"LethalAntiCheat: Refusing connection from kicked player {steamId}.");
                PipeLogger.Log($"[Behaviour] LethalAntiCheat: Refusing connection from kicked player {steamId}.");
                //Console.WriteLine($"LethalAntiCheat: Refusing connection from kicked player {steamId}.");
                return false;
            }

            if (ConnectionIdToSteamIdMap.ContainsKey(connection.Id))
            {
                ConnectionIdToSteamIdMap[connection.Id] = steamId;
            }
            else
            {
                ConnectionIdToSteamIdMap.Add(connection.Id, steamId);
            }

            //Debug.Log($"LethalAntiCheat: Player {new Friend(steamId).Name} is connecting with connection ID {connection.Id}.");
            PipeLogger.Log($"[Behaviour] LethalAntiCheat: Player {new Friend(steamId).Name} is connecting with connection ID {connection.Id}.");
            //Console.WriteLine($"LethalAntiCheat: Player {new Friend(steamId).Name} is connecting with connection ID {connection.Id}.");
            return true;
        }

        private static void OnDisconnectedPrefix(ref Connection connection, ref ConnectionInfo info)
        { 
            if (ConnectionIdToSteamIdMap.ContainsKey(connection.Id))
            {
                PipeLogger.Log($"[Behaviour] LethalAntiCheat: Player disconnected with connection ID {connection.Id}.");
                //Debug.Log($"LethalAntiCheat: Player disconnected with connection ID {connection.Id}.");
                ConnectionIdToSteamIdMap.Remove(connection.Id);
            }
        }
    }
}
