using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;

namespace ReflectionTest
{
    public static class SandboxAppDomain
    {
        private static AppDomain sandbox;

      
        private static readonly HashSet<string> AllowedAssemblies = new()
        {
            "ReflectionTest", // 우리가 만든 안티치트 DLL
            "System",
            "System.Core",
            "mscorlib",
            "netstandard"
        };

        public static void InitializeSandbox()
        {
            Console.WriteLine("[Sandbox] === Current AppDomain Info ===");
            Console.WriteLine($"Name: {AppDomain.CurrentDomain.FriendlyName}");
            Console.WriteLine($"Base Dir: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Loaded Assemblies: {AppDomain.CurrentDomain.GetAssemblies().Length}");

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Console.WriteLine($"  - {asm.FullName}");
            }

            Console.WriteLine("\n[Sandbox] === Creating Isolated AppDomain ===");

            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            sandbox = AppDomain.CreateDomain("IsolatedSandbox", null, setup);
            sandbox.AssemblyLoad += OnAssemblyLoaded;

            Console.WriteLine($"[Sandbox] Isolated AppDomain created: {sandbox.FriendlyName}");
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            var asmName = args.LoadedAssembly.GetName().Name;
            var asmPath = args.LoadedAssembly.Location;

           
            if (AllowedAssemblies.Contains(asmName))
                return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ALERT] New assembly loaded into sandbox: {asmName}");
            Console.WriteLine($"        Location: {(string.IsNullOrEmpty(asmPath) ? "(Unknown)" : asmPath)}");
            Console.ResetColor();

         
        }

        public static void RunInSandbox()
        {
            // 테스트 용도
            Console.WriteLine("[Sandbox] Running... (you may implement specific execution here)");
        }
    }
}
