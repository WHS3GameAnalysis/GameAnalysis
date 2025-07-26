//using System;

//namespace LethalAntiCheatLauncher
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.Title = "Lethal Anti-Cheat Client";
//            Console.Clear();
//            Console.WriteLine("[AntiCheat] Client started. Waiting for game process...\n");

//            PipeListener.Start();
//            InjectorManager.InjectWhenGameStarts();

//            // 유지 (콘솔 자동 새로고침)
//            ConsoleRefresher.Start();

//            while (true)
//                Thread.Sleep(1000);
//        }
//    }
//}
using System;
using System.Threading;
using System.Threading.Tasks;
using LethalAntiCheatLauncher.Integrity;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Lethal Anti-Cheat Client";
            Console.Clear();
            Console.WriteLine("[AntiCheat] Client started. Running integrity check...\n");

            // 1. 무결성 검사
            var checker = new IntegrityChecker();
            var result = checker.CheckIntegrity((cur, total, name, msg) =>
            {
                Console.WriteLine($"[{cur}/{total}] {name} - {msg}");
            });

            Console.WriteLine();
            Console.ForegroundColor = result.IsValid ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(result.Message);
            Console.ResetColor();
            Console.WriteLine();

            if (!result.IsValid)
            {
                Console.WriteLine("[AntiCheat] 게임 실행이 차단되었습니다.");
                return;
            }

            // 하트비트 전송 시작
            HeartbeatManager.Start();

            Console.WriteLine("[AntiCheat] 게임 실행 중...\n");

            // 2. 게임 실행
            if (!checker.LaunchGame())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[AntiCheat] 게임 실행 실패. 관리자 권한 또는 경로 문제일 수 있습니다.");
                Console.ResetColor();
                return;
            }

            // 3. 통신 파이프 리스너 시작
            PipeListener.Start();

            // 4. SimpleAC DLL 로드
            SimpleACManager.LoadSimpleAC();

            // 5. 게임 실행 이후 인젝션 수행
            InjectorManager.InjectWhenGameStarts();

            // 6. 콘솔 주기적 상태 출력
            ConsoleRefresher.Start();

            // 7. 종료 신호 대기 설정
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\n[AntiCheat] 종료 중...");
                Thread.Sleep(1000);
                SimpleACManager.UnloadSimpleAC();
                InjectorManager.UnloadAntiCheat();
                Environment.Exit(0);
            };

            // 8. 무한 루프 대기
            Console.WriteLine("프로그램을 종료하려면 Ctrl+C를 누르세요.\n");
            while (true)
                Thread.Sleep(1000);
        }
    }
}

