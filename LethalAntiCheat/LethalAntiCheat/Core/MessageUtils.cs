
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalAntiCheat.Core
{
    public static class MessageUtils
    {
        private static string lastMessage = string.Empty;

        //인게임 전체 채팅으로 보이는 메시지
        /// <param name="message">The message to display.</param>
        public static void ShowMessage(string message)
        {
            // Prevent message spam.
            if (lastMessage == message) return;
            lastMessage = message;

            string formattedMessage = "<color=red>[AntiCheat] " + message + "</color>";

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.AddTextToChatOnServer(formattedMessage);
            }
        }

        //호스트에게만 보이는 메시지
        public static void ShowHostOnlyMessage(string message)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.AddTextToChatOnServer(message, 0); // 0~3존재. 0번이 호스트.
            }
        }
    }
}
