using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class Minimap : Cheat // Cheat 클래스 상속
    {
        public Camera minimapCamera = null; // 미니맵용 카메라
        public bool isEnabled = false;

        public void Render()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;

            //if (GameNetworkManager.Instance.gameHasStarted)
            {
                if (!GameObject.Find("MinimapCamera")?.GetComponent<Camera>())
                {
                    GameObject cameraObj = new GameObject("MinimapCamera");
                    minimapCamera = cameraObj.AddComponent<Camera>();

                    // 카메라 설정
                    minimapCamera.orthographic = true; // 원근감 제거
                    minimapCamera.orthographicSize = 30f; // 카메라 범위 설정
                    minimapCamera.cullingMask = 0x4000;
                    minimapCamera.clearFlags = CameraClearFlags.Skybox;
                    minimapCamera.backgroundColor = new Color(0.192f, 0.302f, 0.475f, 0.000f);
                    minimapCamera.depth = 10; // 다른 카메라보다 높은 우선순위

                    // 카메라 위치를 플레이어 위치 기준으로 설정
                    minimapCamera.transform.position = new Vector3(localPlayer.transform.position.x, 50f, localPlayer.transform.position.z);
                    minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 아래를 향하도록 회전

                    // 뷰포트를 화면 오른쪽 상단에 설정
                    minimapCamera.rect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);

                    // 카메라 활성화
                    minimapCamera.enabled = false;
                }
                if (Hack.Instance.minimap.isEnabled)
                {
                    if (localPlayer.isInsideFactory) // 공장 내부에 있을 때
                    {
                        minimapCamera.transform.position = new Vector3(localPlayer.transform.position.x, -20f, localPlayer.transform.position.z);
                    }
                    else
                    {
                        minimapCamera.transform.position = new Vector3(localPlayer.transform.position.x, 50f, localPlayer.transform.position.z);
                    }
                    minimapCamera.enabled = true;
                }
                else
                {
                    minimapCamera.enabled = false;
                }
            }
        }

        public override void Trigger()
        {
            throw new NotImplementedException();
        }
    }
}
