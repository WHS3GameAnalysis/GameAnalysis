using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LethalAntiCheatLauncher.Integrity
{
    public static class FileHashUtil
    {
        public static string CalculateSHA256(string filepath)
        {
            if (!File.Exists(filepath)) return string.Empty;

            using FileStream stream = File.OpenRead(filepath);
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(stream);

            StringBuilder sb = new();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}
