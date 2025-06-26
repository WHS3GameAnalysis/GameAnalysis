using GameAnalysis.LethalHack;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Windows;

namespace LethalHack
{
    internal class NoClip : Cheat
    {
        private KBInput movement;

        public override void Trigger() // ✅ 추상 메서드 구현 필수
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

                // ✅ 지속적 위치 갱신
                player.transform.position = movement.transform.position;
            }
            else
            {
                if (movement != null)
                {
                    Object.Destroy(movement);
                    movement = null;
                }

                controller.enabled = true;
            }
        }
    }
}