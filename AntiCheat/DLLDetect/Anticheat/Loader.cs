using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Anticheat
{
    public class Loader
    {
        private static GameObject load;

        public static void Init()
        {
            Loader.load = new GameObject();
            Loader.load.AddComponent<Agent>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.load);
        }
    }

    public class Agent : MonoBehaviour
    {
        public void Start()
        {
            ConsoleBootstrap.InitConsole();
            Console.WriteLine("Scanning...");
            CheckDLL.Start();
            PatchDetector.Start();
        }

        public void Update()
        {

        }

        public void OnGUI()
        {

        }
    }
    /* 콘솔창을 띄우기 위한 클래스 */
    public static class ConsoleBootstrap
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public static void InitConsole()
        {
            AllocConsole();
            Console.Title = "AntiCheat Debug Console";

            var stdout = Console.OpenStandardOutput();
            var writer = new StreamWriter(stdout) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);

            Console.WriteLine("[+] Anti-Cheat Console");
        }
    }
}
