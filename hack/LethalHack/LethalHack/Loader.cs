using BepInEx; 
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LethalHack
{
    [BepInPlugin("com.yourname.lethalhack", "LethalHack", "1.0.0")] 
    public class Loader : BaseUnityPlugin
    {
        private static GameObject load;

        private void Start()
        {
            Init(); // 기존 방식 그대로 호출
        }

        public static void Init() // 기존 구조 유지
        {
            LoadAssembly("LethalHack.Resources.0Harmony.dll");

            load = new GameObject("LethalHackMain");
            load.AddComponent<Hack>();
            UnityEngine.Object.DontDestroyOnLoad(load);
        }

        public static void LoadAssembly(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(name);
            byte[] rawAssembly = new byte[stream.Length];
            stream.Read(rawAssembly, 0, (int)stream.Length);
            AppDomain.CurrentDomain.Load(rawAssembly);
        }
    }
}
