using GameNetcodeStuff;
using UnityEngine;

namespace LethalHack
{
    public class Loader
    {
        public static void Init() // 인젝션 할 때 가장 먼저 실행할 메서드
        {
            Loader.load = new GameObject(); // 새로운 게임 오브젝트 생성
            Loader.load.AddComponent<hack>(); // 게임 오브젝트에 hack 컴포넌트 추가
            UnityEngine.Object.DontDestroyOnLoad(Loader.load); // 가비지 컬렉터 예외 처리(씬이 바뀌어도 파괴되지 않도록 설정)
        }
        private static GameObject load;
    }
    // 게임 오브젝트에 붙일 hack 컴포넌트
    public class hack : MonoBehaviour
    {
        Menu GUIManager = new Menu(); // GUI를 띄우기 위해서 Menu 객체를 하나 만들어줍니다.

        public void Update() // Unity에서 매 프레임마다 호출되는 메서드
        {
            Hack.Instance.Start(); // 매 프레임마다 핵 기능들이 실행됩니다.
        }

        public void OnGUI() // Update 메서드 이후에 호출되는 메서드
        {
            GUI.Label(new Rect(10, 10, 400, 80), "Cheat Enabled"); // 왼쪽 상단 위에 표시할 문구
            GUIManager.Render(); // GUI 렌더링
        }
    }
}