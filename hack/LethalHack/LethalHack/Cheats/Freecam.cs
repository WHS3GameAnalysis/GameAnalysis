using GameNetcodeStuff;
using UnityEngine;
using LethalHack.Manager;
using LethalHack.Input;
using LethalHack.Util;
using LethalHack;

namespace LethalHack.Cheats
{
    public class Freecam : Cheat
    {
        public static PlayerControllerB localPlayer = null;
        public static Camera camera = null;
        private static Light light = null;
        public static MouseInput mouse = null;
        public static KBInput movement = null;
        public static AudioListener audioListener = null;


        public static bool isActive = false;

        public static string debugMessage = "";

        public override void Trigger()
        {
            if (Hack.localPlayer == null)
            {
                Hack.localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (Hack.localPlayer == null) return;
            }
            if (localPlayer == null)
                localPlayer = Hack.localPlayer;

            try
            {
                if (!isActive)
                {
                    Stop();
                    return;
                }

                if (localPlayer != null)
                {
                    localPlayer.enabled = false;
                    localPlayer.isFreeCamera = true;
                }

                CreateIfNull();

                if (light != null)
                {
                    light.intensity = 3000f;
                    light.range = 10000f;
                }

                if (camera != null && movement != null && mouse != null)
                {
                    camera.transform.SetPositionAndRotation(movement.transform.position, mouse.transform.rotation);
                }
            }
            catch (System.Exception e)
            {
                debugMessage = e.Message + "\n" + e.StackTrace;
            }
        }

        private void CreateIfNull()
        {
            if (camera == null)
            {
                Camera baseCamera = CameraManager.GetBaseCamera();
                if (baseCamera == null)
                {
                    Debug.LogError("[Freecam] Cannot create Freecam: Base camera is null.");
                    return;
                }

                camera = GameObjectUtil.CreateCamera("FreeCam", baseCamera.transform);
                camera.enabled = true;
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.cullingMask = -1;
                camera.depth = 100;

                mouse = camera.gameObject.AddComponent<MouseInput>();
                movement = camera.gameObject.AddComponent<KBInput>();
                audioListener = camera.gameObject.AddComponent<AudioListener>();

                light = GameObjectUtil.CreateLight();
                light.transform.SetParent(camera.transform, false);

                baseCamera.enabled = false;
                CameraManager.ActiveCamera = camera;

                Debug.Log("[Freecam] FreeCam camera created and enabled.");
            }
        }

        public static void Reset()
        {
            Stop();
        }

        public static void Stop()
        {
            isActive = false;
            if (StartOfRound.Instance != null)
            {
                Camera baseCamera = CameraManager.GetBaseCamera();
                if (baseCamera != null)
                {
                    CameraManager.ActiveCamera = baseCamera;
                    CameraManager.ActiveCamera.enabled = true;
                }

                if (localPlayer != null)
                {
                    localPlayer.enabled = true;
                    localPlayer.isFreeCamera = false;
                }
            }

            if (camera != null)
            {
                camera.enabled = false;
                UnityEngine.Object.Destroy(camera.gameObject);
                camera = null;
            }

            mouse = null;
            movement = null;

            if (audioListener != null)
            {
                UnityEngine.Object.Destroy(audioListener.gameObject);
                audioListener = null;
            }

            if (light != null)
            {
                UnityEngine.Object.Destroy(light.gameObject);
                light = null;
            }
        }
    }
}