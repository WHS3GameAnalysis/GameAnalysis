
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Netcode.Transports.Facepunch;

namespace LethalAntiCheat.Core
{
    public static class AntiCheatUtils
    {
        // Maps the transport connection ID to the player's SteamID.
        public static Dictionary<uint, ulong> ConnectionIdToSteamIdMap { get; } = new Dictionary<uint, ulong>();

        /// <summary>
        /// Checks if the RPC sender is a valid, non-host player.
        /// </summary>
        /// <param name="senderClientId">The client ID from the RPC parameters.</param>
        /// <param name="player">The PlayerControllerB instance of the sender.</param>
        /// <returns>True if the player is a valid target for cheat checks, false otherwise.</returns>
        public static bool CheckAndGetUser(ulong senderClientId, out PlayerControllerB player)
        {
            player = null;

            // The host is never a target for cheat checks.
            if (senderClientId == NetworkManager.Singleton.LocalClientId)
            {
                return false;
            }

            player = GetPlayerByClientId(senderClientId);

            if (player == null)
            {
                // If we can't find the player, it might be a desync or an invalid client.
                // Disconnecting them is a safe default.
                AntiManager.Instance.KickPlayer(new PlayerControllerB() { playerClientId = senderClientId }, "Invalid player object.");
                return false;
            }

            // Ensure the player hasn't already been kicked.
            if (StartOfRound.Instance.KickedClientIds.Contains(player.playerSteamId))
            {
                return false;
            }

            // Verify SteamID integrity.
            if (player.playerSteamId == 0)
            {
                uint transportId = ClientIdToTransportId(senderClientId);
                if (transportId != 0 && ConnectionIdToSteamIdMap.TryGetValue(transportId, out ulong steamId))
                {
                    Friend f = new Friend(steamId);
                    Debug.Log($"LethalAntiCheat: Player {f.Name}({steamId}) has an invalid SteamID and will be kicked.");
                    AntiManager.Instance.KickPlayer(player, "Invalid SteamID");
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

        /// <summary>
        /// Applies Harmony patches to the FacepunchTransport to monitor player connections.
        /// </summary>
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
                    Debug.LogError("LethalAntiCheat: Failed to find FacepunchTransport.OnConnecting method.");
                }

                if (onDisconnectedMethod != null)
                {
                    harmony.Patch(onDisconnectedMethod, prefix: new HarmonyMethod(typeof(AntiCheatUtils), nameof(OnDisconnectedPrefix)));
                }
                else
                {
                    Debug.LogError("LethalAntiCheat: Failed to find FacepunchTransport.OnDisconnected method.");
                }
            }
            catch (NetworkConfigurationException ex)
            {
                Debug.LogError($"LethalAntiCheat: Exception while patching transport: {ex}");
            }
        }

        private static bool OnConnectingPrefix(ref Connection connection, ref ConnectionInfo info)
        {
            NetIdentity identity = Traverse.Create(info).Field<NetIdentity>("identity").Value;
            ulong steamId = identity.SteamId.Value;

            if (StartOfRound.Instance.KickedClientIds.Contains(steamId))
            {
                Debug.Log($"LethalAntiCheat: Refusing connection from kicked player {steamId}.");
                return false; // Block connection
            }

            if (ConnectionIdToSteamIdMap.ContainsKey(connection.Id))
            {
                ConnectionIdToSteamIdMap[connection.Id] = steamId;
            }
            else
            {
                ConnectionIdToSteamIdMap.Add(connection.Id, steamId);
            }

            Debug.Log($"LethalAntiCheat: Player {new Friend(steamId).Name} is connecting with connection ID {connection.Id}.");
            return true; // Allow connection
        }

        private static void OnDisconnectedPrefix(ref Connection connection, ref ConnectionInfo info)
        { 
            if (ConnectionIdToSteamIdMap.ContainsKey(connection.Id))
            {
                Debug.Log($"LethalAntiCheat: Player disconnected with connection ID {connection.Id}.");
                ConnectionIdToSteamIdMap.Remove(connection.Id);
            }
        }

        //[HarmonyPatch(typeof(StartOfRound), "Start")]
        //[HarmonyPostfix]
        //public static void ResetPlayersRound()
        //{

        //}
    }
}
