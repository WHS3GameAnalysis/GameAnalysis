using GameNetcodeStuff;
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

        public void Render() // Cheat 클래스 MonoBehaviour 상속받았음 -> Render 메서드 자동으로 호출됨
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance?.localPlayerController;

            // 미니맵 카메라가 없으면 새로 생성
            if (minimapCamera == null)
            {
                GameObject cameraObj = new GameObject("MinimapCamera");
                minimapCamera = cameraObj.AddComponent<Camera>();
                    
                // 카메라 설정
                minimapCamera.orthographic = true; // 탑다운 뷰를 위해 직교 투영 사용
                minimapCamera.orthographicSize = 20f; // 카메라 범위 설정
                minimapCamera.cullingMask = ~0; // 모든 레이어 렌더링
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
        }
        public override void Trigger()
        {
            throw new NotImplementedException();
        }
    }
}
