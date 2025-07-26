using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LethalAntiCheatLauncher.Util
{
    public static class HeartbeatManager
    {
        private static bool _started = false;
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string _clientId = HwidUtil.GetHwid();
        private const string SERVER_BASE_URL = "https://ghb.r-e.kr";
        private const string VERSION = "1.0.0";

        public static void Start()
        {
            if (_started) return;

            _started = true;
            Console.WriteLine("[Heartbeat] 하트비트 시작");
            _ = SendHeartbeatLoop();
        }

        private static async Task SendHeartbeatLoop()
        {
            while (true)
            {
                try
                {
                    await PerformHeartbeat();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Heartbeat] 하트비트 실패: {ex.Message}");
                }
                
                await Task.Delay(5000);
            }
        }

        private static async Task PerformHeartbeat()
        {
            // 1단계: 서버로부터 암호화된 챌린지 받기
            var encryptedChallenge = await GetEncryptedChallenge();
            if (string.IsNullOrEmpty(encryptedChallenge))
            {
                Console.WriteLine("[Heartbeat] 챌린지 획득 실패");
                return;
            }

            // 2단계: 챌린지 복호화 및 연산 후 결과 전송
            await SendChallengeResponse(encryptedChallenge);
        }

        private static async Task<string?> GetEncryptedChallenge()
        {
            try
            {
                var response = await _client.GetAsync($"{SERVER_BASE_URL}/heartbeat_init");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var challengeData = JsonSerializer.Deserialize<EncryptedChallenge>(responseText);
                    
                    if (challengeData != null && !string.IsNullOrEmpty(challengeData.EncryptedData))
                    {
                        Console.WriteLine("[Heartbeat] 암호화된 챌린지 수신 성공");
                        return challengeData.EncryptedData;
                    }
                }
                else
                {
                    Console.WriteLine($"[Heartbeat] 챌린지 요청 실패: {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Heartbeat] 챌린지 요청 중 오류: {ex.Message}");
                return null;
            }
        }

        private static async Task SendChallengeResponse(string encryptedChallenge)
        {
            try
            {
                // 1. 클라이언트 개인키로 하이브리드 챌린지 복호화
                var decryptedChallenge = SecurityUtil.DecryptHybridChallenge(encryptedChallenge);
                var challenge = JsonSerializer.Deserialize<HeartbeatChallenge>(decryptedChallenge);
                
                if (challenge == null)
                {
                    Console.WriteLine("[Heartbeat] 챌린지 복호화 실패");
                    return;
                }

                Console.WriteLine($"[Heartbeat] 챌린지 복호화 성공: {challenge.ChallengeId}");

                // 2. 챌린지 유효성 검사
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentTime > challenge.ExpiresAt)
                {
                    Console.WriteLine("[Heartbeat] 챌린지가 만료됨");
                    return;
                }

                // 3. SHA512 알고리즘으로 챌린지 응답 계산
                var responseValue = CalculateChallengeResponse(challenge.ChallengeData);
                
                // 4. 응답 데이터 생성
                var response = new HeartbeatResponse
                {
                    ChallengeId = challenge.ChallengeId,
                    ClientId = _clientId,
                    ResponseValue = responseValue,
                    Timestamp = currentTime,
                    Version = VERSION,
                    SystemFingerprint = SecurityUtil.GenerateSystemFingerprint()
                };

                // 5. 응답을 서버 공개키로 암호화
                var responseJson = JsonSerializer.Serialize(response);
                var encryptedResponse = SecurityUtil.EncryptResponse(responseJson);

                var encryptedRequest = new EncryptedResponse { EncryptedData = encryptedResponse };
                var json = JsonSerializer.Serialize(encryptedRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 6. 서버에 전송
                var httpResponse = await _client.PostAsync($"{SERVER_BASE_URL}/heartbeat_send", content);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseText = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Heartbeat] 응답 전송 성공: {responseText}");
                }
                else
                {
                    Console.WriteLine($"[Heartbeat] 응답 전송 실패: {httpResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Heartbeat] 응답 처리 중 오류: {ex.Message}");
            }
        }

        private static string CalculateChallengeResponse(string challengeData)
        {
            try
            {
                // 시스템 정보와 결합하여 SHA512 해시 생성 (서버와 동일한 방식)
                var systemInfo = SecurityUtil.GenerateSystemFingerprint();
                var combinedData = $"{challengeData}:{systemInfo}:{_clientId}";
                
                using (var sha512 = SHA512.Create())
                {
                    var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(combinedData));
                    return Convert.ToHexString(hash);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Heartbeat] 챌린지 응답 계산 오류: {ex.Message}");
                return string.Empty;
            }
        }
    }

    // 단순화된 데이터 모델들
    public class EncryptedChallenge
    {
        [JsonPropertyName("encrypted_data")]
        public string EncryptedData { get; set; } = string.Empty;
    }

    public class HeartbeatChallenge
    {
        [JsonPropertyName("challenge_id")]
        public string ChallengeId { get; set; } = string.Empty;
        
        [JsonPropertyName("challenge_data")]
        public string ChallengeData { get; set; } = string.Empty;
        
        [JsonPropertyName("algorithm")]
        public string Algorithm { get; set; } = string.Empty;
        
        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }

    public class HeartbeatResponse
    {
        [JsonPropertyName("challenge_id")]
        public string ChallengeId { get; set; } = string.Empty;
        
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;
        
        [JsonPropertyName("response_value")]
        public string ResponseValue { get; set; } = string.Empty;
        
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; } = string.Empty;
    }

    public class EncryptedResponse
    {
        [JsonPropertyName("encrypted_data")]
        public string EncryptedData { get; set; } = string.Empty;
    }
}