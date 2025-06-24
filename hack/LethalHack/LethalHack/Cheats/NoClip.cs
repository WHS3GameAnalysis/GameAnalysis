using GameNetcodeStuff;
using System;
using UnityEngine;
using LethalHack;
using GameAnalysis.LethalHack;

namespace LethalHack
{
    internal class NoClip : Cheat
    {
        private KBInput movement;

        public override void Trigger()
        {
            if (Hack.localPlayer == null) return;

            CharacterController controller = Hack.localPlayer.GetComponent<CharacterController>();
            PlayerControllerB player = Hack.localPlayer;

            if (isEnabled)
            {
                if (movement == null)
                {
                    movement = player.gameObject.AddComponent<KBInput>();
                    movement.transform.position = player.transform.position;
                }

                controller.enabled = false;
                player.transform.position = movement.transform.position;
            }
            else
            {
                if (movement != null)
                {
                    UnityEngine.Object.Destroy(movement);
                    movement = null;
                }

                controller.enabled = true;
            }
        }
    }
}
