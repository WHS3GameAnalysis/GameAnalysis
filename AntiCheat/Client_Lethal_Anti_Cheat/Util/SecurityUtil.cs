using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LethalAntiCheatLauncher.Util
{
    public static class SecurityUtil
    {

        /// <summary>
        /// 클라이언트 개인키로 서버 챌린지 복호화 (기존 RSA 방식)
        /// </summary>
        public static string DecryptChallenge(string encryptedChallenge)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    // 클라이언트 개인키 로드
                    rsa.ImportFromPem(RsaKey.CHAL_PRIVATE_KEY);
                    
                    var encryptedBytes = Convert.FromBase64String(encryptedChallenge);
                    var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                    
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Security] 챌린지 복호화 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 하이브리드 복호화로 서버 챌린지 복호화 (AES + RSA)
        /// </summary>
        public static string DecryptHybridChallenge(string encryptedChallenge)
        {
            try
            {
                // 1. JSON 데이터 파싱
                var hybridData = JsonSerializer.Deserialize<HybridEncryptedData>(encryptedChallenge);
                if (hybridData == null)
                {
                    throw new Exception("하이브리드 데이터 파싱 실패");
                }

                var encryptedKey = Convert.FromBase64String(hybridData.EncryptedKey);
                var iv = Convert.FromBase64String(hybridData.IV);
                var encryptedData = Convert.FromBase64String(hybridData.EncryptedData);

                // 2. RSA로 AES 키 복호화
                byte[] aesKey;
                using (var rsa = RSA.Create())
                {
                    rsa.ImportFromPem(RsaKey.CHAL_PRIVATE_KEY);
                    aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
                }

                // 3. AES로 실제 데이터 복호화
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
                Console.WriteLine($"[Security] 하이브리드 챌린지 복호화 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 하이브리드 암호화로 서버 응답 암호화 (AES + RSA)
        /// </summary>
        public static string EncryptResponse(string plainText)
        {
            try
            {
                // 1. AES 키 생성 (256비트)
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateKey();
                    aes.GenerateIV();

                    // 2. AES로 데이터 암호화
                    byte[] encryptedData;
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        encryptedData = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    }

                    // 3. RSA로 AES 키 암호화
                    using (var rsa = RSA.Create())
                    {
                        rsa.ImportFromPem(RsaKey.RESP_PUBLIC_KEY);
                        var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

                        // 4. 결과 조합 (암호화된 키 + IV + 암호화된 데이터)
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
                Console.WriteLine($"[Security] 응답 암호화 실패: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 안전한 랜덤 문자열 생성
        /// </summary>
        public static string GenerateSecureRandom(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// 시스템 무결성 체크섬 생성
        /// </summary>
        public static string GenerateSystemFingerprint()
        {
            try
            {
                var fingerprint = new StringBuilder();
                
                // CPU 정보
                fingerprint.Append(Environment.ProcessorCount);
                
                // OS 정보
                fingerprint.Append(Environment.OSVersion.ToString());
                
                // 컴퓨터 이름
                fingerprint.Append(Environment.MachineName);
                
                // 사용자 이름
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

        /// <summary>
        /// 메시지 해시 생성
        /// </summary>
        public static string GenerateHash(string message)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToHexString(hash);
            }
        }

        /// <summary>
        /// 하이브리드 암호화 결과 데이터 구조
        /// </summary>
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