using UnityEngine;

namespace LethalHack.Cheats
{
    public class NoVisor : Cheat
    {
        private GameObject helmetModel = null;
        
        public override void Trigger()
        {
            // 헬멧 모델 찾기
            if (helmetModel == null)
            {
                helmetModel = GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/");
            }
            
            // 헬멧 모델 활성화/비활성화인데 비활성화가 안되는 상태.
            if (helmetModel != null)
            {
                helmetModel.SetActive(!isEnabled);
            }
        }
    }
} 