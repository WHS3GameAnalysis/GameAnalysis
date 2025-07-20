using System;
using System.Threading;

namespace LethalAntiCheatLauncher
{
    public static class ConsoleRefresher
    {
        public static void Start()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (InjectorManager.InjectionCompleted)
                    {
                        lock (typeof(Console))
                        {
                            Console.Clear();
                            Console.WriteLine($"[AntiCheat] Console refreshed at {DateTime.Now:T}");
                        }
                    }
                    Thread.Sleep(5000);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
