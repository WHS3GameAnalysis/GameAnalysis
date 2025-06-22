using GameNetcodeStuff;
using System;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication.Internal;
using UnityEngine;

namespace LethalHack.Cheats
{
    public class TeleportImpl
    {
        private static Vector3 savedPosition = Vector3.zero;
        private static bool hasSavedPosition = false;

        public static string SavePosition()
        {
            try
            {
                PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
                if (localPlayer != null)
                {
                    savedPosition = localPlayer.transform.position;
                    hasSavedPosition = true;
                    return $"위치 저장: ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})";
                }
                else
                {
                    return "플레이어를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                return $"위치 저장 실패: {ex.Message}";
            }
        }

        public static string LoadPosition()
        {
            try
            {
                if (!hasSavedPosition)
                {
                    return "저장된 위치가 없습니다.";
                }

                PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
                if (localPlayer != null)
                {
                    localPlayer.transform.position = savedPosition;
                    return $"위치 로드: ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})";
                }
                else
                {
                    return "플레이어를 찾을 수 없습니다.";
                }
            }
            catch (Exception ex)
            {
                return $"위치 로드 실패: {ex.Message}";
            }
        }

        public static string TeleportTo(Vector3 targetPos)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;
            if (localPlayer != null)
            {
                localPlayer.transform.position = targetPos;
                return $"위치 로드: ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})";
            }

            return "플레이어를 찾을 수 없습니다.";
        }
    }

    public class Teleport : Cheat // Cheat 클래스 상속
    {
        private Rect windowRect = new Rect(300, 20, 200, 350);
        private String result = "결과 MSG"; // 결과 메시지 저장용
        private Camera minimapCamera = null; // 미니맵용 카메라

        public void Render() // Cheat 클래스 MonoBehaviour 상속받았음 -> Render 메서드 자동으로 호출됨
        {
            windowRect = GUI.Window(2, windowRect, TeleportMenu, "Teleport");
        }

        private void TeleportMenu(int windowID)
        {
            if (GUI.Button(new Rect(10, 30, 80, 30), "Save Pos"))
            {
                result = TeleportImpl.SavePosition();
            }
            if (GUI.Button(new Rect(110, 30, 80, 30), "Load Pos"))
            {
                //result = TeleportImpl.LoadPosition();
                PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;

                // 미니맵 카메라가 없으면 새로 생성
                if (minimapCamera == null)
                {
                    GameObject cameraObj = new GameObject("MinimapCamera");
                    minimapCamera = cameraObj.AddComponent<Camera>();
                    
                    // 카메라 설정
                    minimapCamera.orthographic = true; // 탑다운 뷰를 위해 직교 투영 사용
                    minimapCamera.orthographicSize = 20f; // 카메라 범위 설정
                    minimapCamera.cullingMask = ~0;
                    minimapCamera.clearFlags = CameraClearFlags.SolidColor;
                    minimapCamera.backgroundColor = Color.black;
                    minimapCamera.depth = 10; // 다른 카메라보다 높은 우선순위
                }

                // 카메라 위치를 플레이어 위치 기준으로 설정
                minimapCamera.transform.position = new Vector3(localPlayer.transform.position.x, 50f, localPlayer.transform.position.z);
                minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 아래를 향하도록 회전
                
                // 뷰포트를 화면 오른쪽 상단에 설정
                minimapCamera.rect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
                
                // 카메라 활성화
                minimapCamera.enabled = true;
                
                result = "미니맵 활성화됨";
            }
            var players = GameNetworkManager.Instance?.localPlayerController.playersManager.allPlayerScripts;
            int y = 70;
            foreach (var player in players)
            {
                if (player && player.IsClient && player != GameNetworkManager.Instance.localPlayerController)
                {
                    // 플레이어 이름과 위치를 표시하는 버튼 생성
                    if (GUI.Button(new Rect(10, y, 180, 30), $"{player.playerUsername} ({player.transform.position.x:F2}, {player.transform.position.y:F2}, {player.transform.position.z:F2})"))
                    {
                        TeleportImpl.TeleportTo(player.transform.position);
                    }
                    y += 40; // 다음 버튼 위치 조정
                }
            }

            // 결과 메시지를 표시
            GUI.Label(new Rect(10, y, 180, 60), result);

            GUI.DragWindow(); // GUI 창을 마우스로 드래그할 수 있게 해줌
        }

        public override void Trigger()
        {
            throw new NotImplementedException();
        }
    }
}
