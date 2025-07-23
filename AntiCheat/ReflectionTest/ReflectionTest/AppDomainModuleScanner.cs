using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace ReflectionTest
{
    public static class AppDomainModuleScanner
    {
        private static readonly HashSet<string> AllowedAssemblyPrefixes = new()
        {
            "UnityEngine", "Assembly-CSharp", "System", "mscorlib", "netstandard", "ReflectionTest"
        };

        private static readonly HashSet<string> AllowedAssemblyHashes = new();
        private static string SelfAssemblyHash = "";
        private static readonly string AllowedRootPath =
            Path.GetFullPath(@"C:\Program Files (x86)\Steam\steamapps\common\Lethal Company").ToLower();

        public static void Initialize()
        {
            Console.WriteLine("[Init] Collecting allowed DLL hash values...");

            // 1) 게임 기본 DLL 해시 수집
            string managedDir = Path.Combine(AllowedRootPath, "Lethal Company_Data", "Managed");
            var hashes = HashDumper.DumpHashesFromDirectory(managedDir);
            foreach (var hash in hashes)
                AllowedAssemblyHashes.Add(hash);

            // 2) 현재 안티치트 DLL(자기 자신)을 허용 목록에 추가
            try
            {
                string selfPath = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(selfPath) && File.Exists(selfPath))
                {
                    SelfAssemblyHash = ComputeSHA256(selfPath);
                    AllowedAssemblyHashes.Add(SelfAssemblyHash);
                    Console.WriteLine($"[Init] Self-assembly allowed: {Path.GetFileName(selfPath)} = {SelfAssemblyHash}");
                }
                else
                {
                    Console.WriteLine("[Init] Warning: Self-assembly path is empty or invalid.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Init] Failed to add self assembly hash: {ex.Message}");
            }

            Console.WriteLine($"[Init] Total allowed assemblies: {AllowedAssemblyHashes.Count}\n");
        }


        public static void Scan()
        {
            Console.WriteLine("[AppDomain] Scanning loaded assemblies...");
            int detectedCount = 0;
            Assembly selfAsm = Assembly.GetExecutingAssembly();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 자기 자신 Assembly는 무시
                if (asm == selfAsm)
                    continue;

                string fullName = asm.FullName;
                string location = "(No Location Info)";
                string hash = "(Unknown Hash)";

                try
                {
                    location = asm.Location;
                    if (!string.IsNullOrEmpty(location) && File.Exists(location))
                        hash = ComputeSHA256(location);
                }
                catch { }

                bool isNameAllowed = AllowedAssemblyPrefixes.Any(prefix => fullName.StartsWith(prefix));
                bool isHashAllowed = AllowedAssemblyHashes.Contains(hash);
                bool isPathAllowed = !string.IsNullOrEmpty(location) &&
                                     location.StartsWith(AllowedRootPath, StringComparison.OrdinalIgnoreCase);

                if ((!isNameAllowed && !isHashAllowed) || !isPathAllowed)
                {
                    detectedCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ALERT] Suspicious assembly detected:");
                    Console.ResetColor();
                    Console.WriteLine($"         Name     : {fullName}");
                    Console.WriteLine($"         Location : {location}");
                    Console.WriteLine($"         SHA256   : {hash}");
                    Console.WriteLine($"         PID      : {Process.GetCurrentProcess().Id}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("[AppDomain] Scan complete.");
            Console.WriteLine($"[AppDomain] Total suspicious assemblies detected: {detectedCount}\n");
        }

        private static string ComputeSHA256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
