using System;
using System.Diagnostics;
using System.Timers;

namespace Anticheat
{
    public static class CheckDLL
    {
        private static Timer timer;

        public static void Start()
        {
            timer = new Timer(5000); // 5초마다 체크
            timer.Elapsed += CheckModules;
            timer.AutoReset = true;
            timer.Start();
        }
        /* 프로세스 내에 로드된 모듈의 목록을 가져오는 함수 */
        private static void CheckModules(object sender, ElapsedEventArgs e)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    // AllowList에 포함되지 않은 모듈일 경우에만 출력
                    if (!AllowList.moduleList.Contains(module.ModuleName))
                    {
                        // 콘솔창에 출력
                        Console.WriteLine("\n[!] Detected Module :");
                        Console.WriteLine("- " + module.ModuleName);
                    }      
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("[X] Error: " + ex.Message);
            }
        }
    }
}
