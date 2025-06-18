using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GHBCheat.Themes
{
    public class Theme
    {
        public static string name { get; private set; }
        public static GUISkin Skin { get; private set; }
        public static AssetBundle AssetBundle;

        public static void Initialize() => SetTheme(string.IsNullOrEmpty(name) ? "Default" : name);

        public static void SetTheme(string t) => LoadTheme(name = ThemeExists(t) ? t : "Default");

        private static bool ThemeExists(string t)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"GHBCheat.Resources.Theme.{t}.skin") != null;
        }

        private static AssetBundle LoadAssetBundle(string r)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(r);
            return stream != null ? AssetBundle.LoadFromStream(stream) : null;
        }

        public static string[] GetThemes()
        {
            var prefix = "GHBCheat.Resources.Theme.";
            var suffix = ".skin";
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(r => r.StartsWith(prefix) && r.EndsWith(suffix))
                .Select(r => r.Substring(prefix.Length, r.Length - prefix.Length - suffix.Length))
                .OrderBy(n => n)
                .ToArray();
        }

        private static void LoadTheme(string t)
        {
            AssetBundle?.Unload(true);
            AssetBundle = null;
            Skin = null;

            string resourcePath = $"GHBCheat.Resources.Theme.{t}.skin";
            AssetBundle = LoadAssetBundle(resourcePath);

            if (AssetBundle == null)
            {
                Debug.LogError($"[ERROR] Failed to load theme file => {resourcePath}");
                return;
            }

            Skin = AssetBundle.LoadAllAssets<GUISkin>().FirstOrDefault();
            if (Skin == null) return;

            Debug.Log($"Loaded Theme {t}");
        }
    }
}
