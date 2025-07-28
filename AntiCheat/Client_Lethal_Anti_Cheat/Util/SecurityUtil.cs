using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher.Util
{
    public static class SecurityUtil
    {
        public static string DecryptHybridChallenge(string encryptedChallenge)
        {
            try
            {
                var hybridData = JsonSerializer.Deserialize<HybridEncryptedData>(encryptedChallenge);
                if (hybridData == null)
                {
                    throw new Exception("Hybrid data parsing failed");
                }

                var encryptedKey = Convert.FromBase64String(hybridData.EncryptedKey);
                var iv = Convert.FromBase64String(hybridData.IV);
                var encryptedData = Convert.FromBase64String(hybridData.EncryptedData);

                byte[] aesKey;
                using (var rsa = RSA.Create())
                {
                    rsa.ImportFromPem(RsaKey.CHAL_PRIVATE_KEY);
                    aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
                }

                using (var aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.AntiCheat, $"[Security] Hybrid challenge decryption failed: {ex.Message}", Color.Red);
                throw;
            }
        }

        public static string EncryptResponse(string plainText)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateKey();
                    aes.GenerateIV();

                    byte[] encryptedData;
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        encryptedData = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    }

                    using (var rsa = RSA.Create())
                    {
                        rsa.ImportFromPem(RsaKey.RESP_PUBLIC_KEY);
                        var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

                        var hybridData = new HybridEncryptedData
                        {
                            EncryptedKey = Convert.ToBase64String(encryptedKey),
                            IV = Convert.ToBase64String(aes.IV),
                            EncryptedData = Convert.ToBase64String(encryptedData)
                        };

                        return JsonSerializer.Serialize(hybridData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.AntiCheat, $"[Security] Response encryption failed: {ex.Message}", Color.Red);
                throw;
            }
        }

        public static string GenerateSystemFingerprint()
        {
            try
            {
                var fingerprint = new StringBuilder();
                fingerprint.Append(Environment.ProcessorCount);
                fingerprint.Append(Environment.OSVersion.ToString());
                fingerprint.Append(Environment.MachineName);
                fingerprint.Append(Environment.UserName);

                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint.ToString()));
                    return Convert.ToHexString(hash);
                }
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        public class HybridEncryptedData
        {
            [JsonPropertyName("encrypted_key")]
            public string EncryptedKey { get; set; } = string.Empty;
            
            [JsonPropertyName("iv")]
            public string IV { get; set; } = string.Empty;
            
            [JsonPropertyName("encrypted_data")]
            public string EncryptedData { get; set; } = string.Empty;
        }
    }
}
