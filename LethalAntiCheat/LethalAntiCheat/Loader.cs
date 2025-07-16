using LethalAntiCheat.Core;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LethalAntiCheat
{
    public class Loader
    {
        private static GameObject antiCheatManagerObject;

        public static void Init()
        {
            LoadAssembly("LethalAntiCheat.Libs.0Harmony.dll");

            Loader.antiCheatManagerObject = new GameObject("LethalAntiCheatManager");
            Loader.antiCheatManagerObject.AddComponent<AntiManager>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.antiCheatManagerObject);
        }

        public static void LoadAssembly(string name)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    Debug.LogError($"LethalAntiCheat: Failed to load embedded resource: {name}");
                    return;
                }
                byte[] rawAssembly = new byte[stream.Length];
                stream.Read(rawAssembly, 0, (int)rawAssembly.Length);
                Assembly.Load(rawAssembly);
            }
        }
    }
}

