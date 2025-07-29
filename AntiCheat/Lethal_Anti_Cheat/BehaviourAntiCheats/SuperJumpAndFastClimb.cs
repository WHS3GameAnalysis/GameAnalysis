
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Anti_Cheat.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Lethal_Anti_Cheat.Util;
using System;

namespace Lethal_Anti_Cheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB), "__rpc_handler_2013428264")] // UpdatePlayerPositionServerRpc
    public static class SuperJumpAndFastClimb
    {
        private static readonly Dictionary<ulong, float> climbTracker = new Dictionary<ulong, float>();
        private static readonly Dictionary<ulong, float> jumpTracker = new Dictionary<ulong, float>();
        private static readonly Dictionary<ulong, float> climbStartTimes = new Dictionary<ulong, float>();

        private const float MAX_CLIMB_SPEED = 13.0f;
        private const float MAX_VERTICAL_SPEED = 108.0f;
        private const float CLIMB_DETECTION_DURATION = 0.3f;

        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, FastBufferReader reader, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var player)) { return true; }

            var startPos = reader.Position;
            reader.ReadValueSafe(out Vector3 newPos);
            reader.Seek(startPos);

            if (player.isClimbingLadder)
            {
                if (jumpTracker.ContainsKey(player.playerSteamId))
                {
                    jumpTracker.Remove(player.playerSteamId);
                }

                if (!climbStartTimes.ContainsKey(player.playerSteamId))
                {
                    climbStartTimes[player.playerSteamId] = Time.time;
                }

                if (climbStartTimes.TryGetValue(player.playerSteamId, out float startTime) && Time.time - startTime > CLIMB_DETECTION_DURATION)
                {
                    return true;
                }

                if (climbTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_CLIMB_SPEED)
                    {
                        return Kick(player, "Fast Climb", speed, climbTracker);
                    }
                }
                climbTracker[player.playerSteamId] = newPos.y;
            }
            else
            {
                if (climbTracker.ContainsKey(player.playerSteamId))
                {
                    climbTracker.Remove(player.playerSteamId);
                }
                if (climbStartTimes.ContainsKey(player.playerSteamId))
                {
                    climbStartTimes.Remove(player.playerSteamId);
                }

                if (jumpTracker.TryGetValue(player.playerSteamId, out float lastY))
                {
                    float speed = (newPos.y - lastY) / Time.deltaTime;
                    if (speed > MAX_VERTICAL_SPEED)
                    {
                        return Kick(player, "Super Jump / teleport", speed, jumpTracker);
                    }
                }
                jumpTracker[player.playerSteamId] = newPos.y;
            }
            return true;
        }

        private static bool Kick(PlayerControllerB player, string hackName, float speed, Dictionary<ulong, float> tracker)
        {
            PipeLogger.Log($"[Behaviour][{hackName}] {player.playerUsername} has abnormal speed: {speed:F2} m/s");

            

            AntiManager.KickPlayer(player, $"{hackName} Hack");
            tracker.Remove(player.playerSteamId);
            
            if (climbStartTimes.ContainsKey(player.playerSteamId))
            {
                climbStartTimes.Remove(player.playerSteamId);
            }

            return false;
        }
    }
}
