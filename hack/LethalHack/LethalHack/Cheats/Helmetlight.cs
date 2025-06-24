/*using System;
using UnityEngine;

namespace LethalHack
{
    internal class Helmetlight
    {
        private Light helmetLight; // 헬멧 라이트 참조

        public void ToggleHelmetLight()
        {
            // Hack 클래스에서 helmetLight를 가져오거나, 직접 참조를 설정
            if (helmetLight != null)
            {
                helmetLight.enabled = !helmetLight.enabled;
                // 네트워크 동기화
                ToggleHelmetLightServerRpc(helmetLight.enabled);
            }
        }

        // 헬멧 라이트 참조 설정
        public void SetHelmetLight(Light light)
        {
            this.helmetLight = light;
        }

        // 네트워크 RPC 메서드 (예시)
        private void ToggleHelmetLightServerRpc(bool enabled)
        {
            // 서버 RPC 구현
        }
    }
}*/