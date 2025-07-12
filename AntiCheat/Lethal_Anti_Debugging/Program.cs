using Lethal_Anti_Debugging.DebugDetector;
using Lethal_Anti_Debugging.ProcessWatcher;
using System;

namespace Lethal_Anti_Debugging
{
    class Program
    {
        static void Main()
        {
            string gameName = "Lethal Company";
            var gameProcess = Lethal_Anti_Debugging.GameScanner.FindGame(gameName);

            if (gameProcess == null)
            {
                Console.WriteLine($"[ERROR] Game '{gameName}' not found.");
                return;
            }
            else
            {
                Console.WriteLine($"Game '{gameName}' found with Process ID: {gameProcess.Id}");
            }

            // Start monitoring for suspicious processes
            ProcessWatcher.ProcessWatcher.StartMonitoring();

            var checkers = new List<IDebugCheck>
        {
            new RemoteDebuggerCheck(),
            new NtQueryCheck(),
            new OutputDebugStringCheck(),
            new AppDomainAssemblyCheck(), // Assuming you have an AppDomainCheck class
            new MonoPortScanCheck(), // Assuming you have a MonoPortScanCheck class
            //new MonoDebuggerAttachCheck(), // Assuming you have a MonoDebuggerAttachCheck class
            // You can add more checks here if needed
        };
            // 주기적으로 디버깅 상태 감지
            while (true)
            {
                Console.WriteLine($"\n[{DateTime.Now}]");
                foreach (var checker in checkers)
                {
                    bool isDebugged = checker.IsDebugged(gameProcess);
                    Console.WriteLine($"{checker.MethodName}: Debugged? {isDebugged}");
                }

                Thread.Sleep(5000); // 5초 간격으로 검사
            }
        }
    }
}