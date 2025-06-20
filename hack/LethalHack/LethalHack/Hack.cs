using LethalHack.Cheats;
using System.Collections.Generic;

namespace LethalHack
{
    public abstract class Cheat // 세부 기능을 구현할 때 사용할 추상 클래스
    {
        public bool isEnabled = true; // On/Off
        public abstract void Trigger(); // Trigger 메서드로 기능 실행
    }

    public class Hack // 기능을 실행하기 위해 사용되는 클래스
    {
        public static Hack Instance = new Hack(); // 싱글톤으로 외부에서도 참조 가능하게 만들었습니다.
        // 기능의 인스턴스를 만들고,
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        internal HPDisplay Hpdisp = new HPDisplay();

        // Start 메서드에서 On/Off 여부에 따라 기능을 실행합니다.
        public void Start()
        {
            if (God.isEnabled) God.Trigger();
            if (Stamina.isEnabled) Stamina.Trigger();
            if (Hpdisp.isEnabled) Hpdisp.Trigger();
        }
    }
}