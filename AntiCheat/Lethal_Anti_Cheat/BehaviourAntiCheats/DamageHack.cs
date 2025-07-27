
using GameNetcodeStuff;
using HarmonyLib;
using Lethal_Anti_Cheat.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.AntiCheats
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public static class DamageHack
    {
        public static readonly HashSet<ulong> warnedPlayers = new HashSet<ulong>();

        private static bool IsShovel(GrabbableObject item) => item is Shovel;
        private static bool IsKnife(GrabbableObject item) => item is KnifeItem;

        [HarmonyPatch("__rpc_handler_638895557")] 
        [HarmonyPrefix]
        public static bool Prefix(NetworkBehaviour __instance, FastBufferReader reader, __RpcParams rpcParams)
        {
            if (!AntiCheatUtils.CheckAndGetUser(rpcParams.Server.Receive.SenderClientId, out var attacker))
            {
                return false; 
            }

            var victim = __instance as PlayerControllerB;


            var startPos = reader.Position;
            ByteUnpacker.ReadValueBitPacked(reader, out int damageAmount);
            reader.ReadValueSafe(out Vector3 hitDirection);
            ByteUnpacker.ReadValueBitPacked(reader, out int playerWhoHit);
            reader.Seek(startPos);

            return CheckDamage(victim, attacker, ref damageAmount);
        }

        private static bool CheckDamage(PlayerControllerB victim, PlayerControllerB attacker, ref int damageAmount)
        {
            if (damageAmount == 0)
            {
                return true; 
            }

            if (warnedPlayers.Contains(attacker.playerSteamId))
            {
                damageAmount = 0;
                return false;
            }

            var heldItem = attacker.ItemSlots[attacker.currentItemSlot];
            float distance = Vector3.Distance(attacker.transform.position, victim.transform.position);

            string reason = null;

            // Check 1: 데미지 값이 다름
            if (heldItem != null && (IsShovel(heldItem) || IsKnife(heldItem)) && damageAmount != 20)
            {
                reason = $"Damage value mismatch. Expected 20, got {damageAmount}";
            }
            // Check 2: 원격으로 공격함
            else if (distance > 12f && heldItem != null && (IsShovel(heldItem) || IsKnife(heldItem)))
            {
                reason = $"Remote attack detected. Distance: {distance:F1}m";
            }
            // Check 3: 무기 없지만 공격함
            else if (heldItem == null)
            {
                reason = "Attacked with an empty hand.";
            }

            if (reason != null)
            {
                PipeLogger.Log($"[Behaviour][DamageHack] {attacker.playerUsername} tried to use a Damage Hack. ({reason})");
                AntiManager.KickPlayer(attacker, "Damage Hack");
                warnedPlayers.Add(attacker.playerSteamId);
                damageAmount = 0; // 데미지 0으로 돌림
                return false; 
            }

            return true; 
        }
    }
}
