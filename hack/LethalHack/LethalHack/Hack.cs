using LethalHack.Cheats;
using System.Collections.Generic;

namespace LethalHack
{
    public abstract class Cheat
    {
        public bool isEnabled = false;
        public abstract void Trigger();
    }

<<<<<<< Updated upstream
    public class Hack // 기능을 실행하기 위해 사용되는 클래스
    {
        public static Hack Instance = new Hack(); // 싱글톤으로 외부에서도 참조 가능하게 만들었습니다.
        // 기능의 인스턴스를 만들고,
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        internal HPDisplay Hpdisp = new HPDisplay();
=======
    public class Hack : MonoBehaviour
    {
        public static Hack Instance;
        public static PlayerControllerB localPlayer;
        // Cheat 기능들 선언
        public GodMode God = new GodMode();
        public InfinityStamina Stamina = new InfinityStamina();
        internal HPDisplay Hpdisp = new HPDisplay();
        internal SuperJump SuperJump = new SuperJump();
        public Teleport teleport = new Teleport();
        public DamageHack damageHack = new DamageHack();
        public Minimap minimap = new Minimap();
        public Freecam freecam = new Freecam();
        public static Harmony harmony;

>>>>>>> Stashed changes

        Menu GUIManager = new Menu();

        public void Start()
        {
<<<<<<< Updated upstream
            if (God.isEnabled) God.Trigger();
            if (Stamina.isEnabled) Stamina.Trigger();
            if (Hpdisp.isEnabled) Hpdisp.Trigger();
=======
            Instance = this;
            localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer == null) return;
            HarmonyPatching();
        }

        private void HarmonyPatching()
        {
            harmony = new Harmony("LethalHack");
            Harmony.DEBUG = false;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsDefined(typeof(HarmonyPatch), false))
                {
                    try
                    {
                        new PatchClassProcessor(harmony, type).Patch();
                    }
                    catch
                    {
                        Debug.LogWarning($"Skipping patch in {type.FullName}");
                    }
                }
            }
        }

        public void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 80), "Cheat Enabled");
            GUIManager.Render();
        }


        public void Update()
        {
            if (God.isEnabled) God.Trigger();
            if (Stamina.isEnabled) Stamina.Trigger();
            if (Hpdisp.isEnabled) Hpdisp.Trigger();
            if(freecam.isEnabled) freecam.Trigger();
            // if (SuperJump.isEnabled) SuperJump.Trigger();

>>>>>>> Stashed changes
        }
    }
}