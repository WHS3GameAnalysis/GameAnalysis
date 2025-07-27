
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace Lethal_Anti_Cheat.Core
{
    public static class MessageUtils
    {
        private static string lastMessage = string.Empty;

        public static void ShowMessage(string message)
        {
            string formattedMessage = "<color=red>[LethalAntiCheat] " + message + "</color>";

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.AddTextToChatOnServer(formattedMessage);
            }
        }

        public static void ShowHostOnlyMessage(string message)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.AddTextToChatOnServer(message, 0);
            }
        }
    }
}
