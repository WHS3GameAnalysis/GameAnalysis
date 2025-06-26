using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LethalHack
{
    public class Loader
    {
        private static GameObject load;

        public static void Init() // 인젝션 할 때 가장 먼저 실행할 메서드
        {
            LoadAssembly("LethalHack.Resources.0Harmony.dll");
            Loader.load = new GameObject(); // 새로운 게임 오브젝트 생성
            Loader.load.AddComponent<Hack>(); // 게임 오브젝트에 hack 컴포넌트 추가
            UnityEngine.Object.DontDestroyOnLoad(Loader.load); // 가비지 컬렉터 예외 처리(씬이 바뀌어도 파괴되지 않도록 설정)
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