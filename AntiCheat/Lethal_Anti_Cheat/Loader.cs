using System;
using Lethal_Anti_Cheat.Util;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.ProcessWatcher;


namespace Lethal_Anti_Cheat
{
    public class Loader
    {
        public static void Init()
        {
            //ConsoleManager.Initialize();
            //Console.Clear();
            //Console.WriteLine("[AntiCheat] Module Init 완료.");

            DebugDetector.DebugDetector.Init(); // 체크 리스트 초기화
            UnifiedScanner.Start(); // 모든 감지 루프 시작
        }
    }
}
