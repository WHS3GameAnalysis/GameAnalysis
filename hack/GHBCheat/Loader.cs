﻿//using GHBCheat.Handler;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;


namespace GHBCheat
{
    public class Loader : MonoBehaviour
    {
        private static GameObject Load;

        public static void Init()
        {
            if (Load != null) return;
            LoadAssembly("GHBCheat.Resources.0Harmony.dll");
            //ChamHandler.ChamsSetEnabled(true);
            Load = new GameObject();
            Load.AddComponent<GHBCheat>();
            Object.DontDestroyOnLoad(Load);
        }

        public static void LoadAssembly(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(name);
            byte[] rawAssembly = new byte[stream.Length];
            stream.Read(rawAssembly, 0, (int)stream.Length);
            AppDomain.CurrentDomain.Load(rawAssembly);
        }

        public static void Unload()
        {
            HackExtensions.ToggleFlags.Keys.ToList().ForEach(h => HackExtensions.ToggleFlags[h] = false);
            //ChamHandler.ChamsSetEnabled(false);
            if ((bool)!GHBCheat.localPlayer?.playerActions.Movement.enabled) GHBCheat.localPlayer?.playerActions.Enable();
            //if (Cursor.visible && !GHBCheat.quickMenuManager.isMenuOpen) Cursor.visible = false;
            //GHBCheat.harmony.UnpatchAll("GHBCheat");
            Object.Destroy(Load);
        }
    }
}