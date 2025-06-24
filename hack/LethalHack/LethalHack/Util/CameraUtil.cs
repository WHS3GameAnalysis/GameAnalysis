using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalHack.Util
{
    public class CameraUtil : MonoBehaviour
    {
        public static bool WorldToScreen(Camera camera, Vector3 world, out Vector3 screen)
        {
            screen = camera.WorldToViewportPoint(world);
            screen.x *= (float)Screen.width;
            screen.y *= (float)Screen.height;
            screen.y = (float)Screen.height - screen.y;
            return (double)screen.z > 0.0;
        }

        public static bool WorldToScreen(Vector3 world, out Vector3 screen)
        {
            screen = Hack.localPlayer.gameplayCamera.WorldToViewportPoint(world);
            screen.x *= (float)Screen.width;
            screen.y *= (float)Screen.height;
            screen.y = (float)Screen.height - screen.y;
            return (double)screen.z > 0.0;
        }

        public static float GetDistanceToPlayer(Vector3 position)
        {
            return Hack.localPlayer.gameplayCamera != null ? (float)Math.Round((double)Vector3.Distance(Hack.localPlayer.gameplayCamera.transform.position, position)) : 0f;
        }
    }
}
