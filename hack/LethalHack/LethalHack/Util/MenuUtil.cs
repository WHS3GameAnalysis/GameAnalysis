using UnityEngine;

namespace LethalHack.Util
{
    /// <summary>
    /// 마우스 커서 토글을 위한 스크립트입니다.
    /// Tab 키를 누르면 마우스 커서의 잠금 상태와 가시성을 토글합니다.
    /// </summary>
    public class MenuUtil
    {
        public static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) // Tab 키로 토글
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true; // GUI 조작 가능
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false; // 게임 시야 조작
                }
            }
        }
    }
}