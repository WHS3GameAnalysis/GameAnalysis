using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ReflectionTest
{
    internal static class HashDumper
    {
        public static HashSet<string> DumpHashesFromDirectory(string directoryPath)
        {
            var hashes = new HashSet<string>();

            if (!Directory.Exists(directoryPath))
                return hashes;

            var dllFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in dllFiles)
            {
                try
                {
                    string hash = ComputeSHA256(file);
                    hashes.Add(hash);
                }
                catch
                {
                    // 오류 무시 
                }
            }

            return hashes;
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

