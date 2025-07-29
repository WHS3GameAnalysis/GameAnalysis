using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.Reflection
{
    public static class SandboxAppDomain
    {
        private static AppDomain sandbox;


        private static readonly HashSet<string> AllowedAssemblies = new()
        {
            "Lethal_Anti_Cheat", // 우리가 만든 안티치트 DLL
            "System",
            "System.Core",
            "mscorlib",
            "netstandard"
        };

        public static void InitializeSandbox()
        {
            PipeLogger.Log("[Reflection] === Current AppDomain Info ===");
            PipeLogger.Log($"[Reflection] Name: {AppDomain.CurrentDomain.FriendlyName}");
            PipeLogger.Log($"[Reflection] Base Dir: {AppDomain.CurrentDomain.BaseDirectory}");
            PipeLogger.Log($"[Reflection] Loaded Assemblies: {AppDomain.CurrentDomain.GetAssemblies().Length}");

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                PipeLogger.Log($"[Reflection]  - {asm.FullName}");
            }

            PipeLogger.Log("\n[Reflection] === Creating Isolated AppDomain ===");

            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            sandbox = AppDomain.CreateDomain("IsolatedSandbox", null, setup);
            sandbox.AssemblyLoad += OnAssemblyLoaded;

            PipeLogger.Log($"[Reflection] Isolated AppDomain created: {sandbox.FriendlyName}");
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            var asmName = args.LoadedAssembly.GetName().Name;
            var asmPath = args.LoadedAssembly.Location;


            if (AllowedAssemblies.Contains(asmName))
                return;

            PipeLogger.Log($"[Reflection] New assembly loaded into sandbox: {asmName}");
            PipeLogger.Log($"[Reflection]        Location: {(string.IsNullOrEmpty(asmPath) ? "(Unknown)" : asmPath)}");


        }

        public static void RunInSandbox()
        {
            // 테스트 용도
            PipeLogger.Log("[Reflection] Running... (you may implement specific execution here)");
        }
    }
}
