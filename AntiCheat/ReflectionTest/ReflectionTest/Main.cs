using System;
using System.Threading;
using System.IO;

namespace ReflectionTest
{
    public class Main
    {
        public static void Init()
        {
            ConsoleManager.AllocConsole();
            ConsoleManager.SetupOut();
            ConsoleManager.SetupIn();

            Console.WriteLine("[AntiCheat] Console has been reset");
            Console.WriteLine("[AntiCheat] AntiCheat Thread Running...");

            AppDomainModuleScanner.Initialize();

            new Thread(() => 
            {
                while (true)
                {
                    AppDomainModuleScanner.Scan();
                    Thread.Sleep(10000); // 10초 주기
                }
            }).Start();
            //Dll 인젝션 방지 스레드 시작
            new Thread(() =>
            {
                try
                {
                    Detector.StartScheduledHashScan();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] {ex.Message}");
                }
            }).Start();
            //함수 변조 방지 스레드 시작
            try
            {
                SandboxAppDomain.InitializeSandbox();
                Console.WriteLine("[Sandbox] Sandbox initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {ex.Message}");
            }
        }
    }
}
