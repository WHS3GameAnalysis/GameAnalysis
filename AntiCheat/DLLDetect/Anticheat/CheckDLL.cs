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

        private static void CheckModules(object sender, ElapsedEventArgs e)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    if (!AllowList.moduleList.Contains(module.ModuleName))
                    {
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
