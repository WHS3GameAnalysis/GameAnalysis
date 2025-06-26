using GameNetcodeStuff;
using UnityEngine;

namespace LethalHack.Manager
{
    public class CameraManager
    {
        
        private static Camera _camera = null;
        public static Camera ActiveCamera
        {
            get
            {
                if (!(bool)StartOfRound.Instance) _camera = null;
                if(_camera == null || UsingBaseCamera()) _camera = GetBaseCamera();
                return _camera;
            }
            set
            {
                _camera = value;
            }
        }
        
        public static Camera GetBaseCamera()
        {
            if (Hack.localPlayer == null || Hack.localPlayer.gameplayCamera == null) return Camera.main;
            return Hack.localPlayer.isPlayerDead ? StartOfRound.Instance.spectateCamera : Hack.localPlayer.gameplayCamera ?? Camera.main;
        }

        public static bool UsingBaseCamera()
        {
            return _camera.GetInstanceID() == Hack.localPlayer?.gameplayCamera.GetInstanceID() || _camera.GetInstanceID() == StartOfRound.Instance.spectateCamera.GetInstanceID();
        }     
    }
}
