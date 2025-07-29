using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.Reflection
{
    public static class AppDomainModuleScanner
    {
        private static readonly HashSet<string> AllowedAssemblyPrefixes = new()
        {
            "UnityEngine", "Assembly-CSharp", "System", "mscorlib", "netstandard", "Lethal_Anti_Cheat"
        };

        private static readonly HashSet<string> AllowedAssemblyHashes = new();
        private static string SelfAssemblyHash = "";
        private static readonly string AllowedRootPath =
            //Path.GetFullPath(@"C:\Program Files (x86)\Steam\steamapps\common\Lethal Company").ToLower();
            Path.GetFullPath(@"C:\SteamLibrary\steamapps\common\Lethal Company").ToLower(); // 이지훈 환경

        public static void Initialize()
        {
            PipeLogger.Log("[Reflection] Collecting allowed DLL hash values...");

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
                    PipeLogger.Log($"[Reflection] Self-assembly allowed: {Path.GetFileName(selfPath)} = {SelfAssemblyHash}");
                }
                else
                {
                    PipeLogger.Log("[Reflection] Warning: Self-assembly path is empty or invalid.");
                }
            }
            catch (Exception ex)
            {
                PipeLogger.Log($"[Reflection] Failed to add self assembly hash: {ex.Message}");
            }

            PipeLogger.Log($"[Reflection] Total allowed assemblies: {AllowedAssemblyHashes.Count}\n");
        }


        public static void Scan()
        {
            PipeLogger.Log("[Reflection] Scanning loaded assemblies...");
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
                    PipeLogger.Log($"[Reflection] Suspicious assembly detected:");
                    PipeLogger.Log($"[Reflection]         Name     : {fullName}");
                    PipeLogger.Log($"[Reflection]         Location : {location}");
                    PipeLogger.Log($"[Reflection]         SHA256   : {hash}");
                    PipeLogger.Log($"[Reflection]         PID      : {Process.GetCurrentProcess().Id}");
                }
            }

            PipeLogger.Log("[Reflection] AppDomain Scan complete.");
            PipeLogger.Log($"[Reflection] Total suspicious assemblies detected: {detectedCount}\n");
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
