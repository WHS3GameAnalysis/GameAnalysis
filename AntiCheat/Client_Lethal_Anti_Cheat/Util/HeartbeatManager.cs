using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Drawing;
using System.Text.Json.Serialization;
using LethalAntiCheatLauncher.Util;


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
            LogManager.Log(LogSource.Heartbeat, "Starting heartbeat...", Color.Gray);
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
                    LogManager.Log(LogSource.Heartbeat, $"Heartbeat failed: {ex.Message}", Color.Red);
                }
                
                await Task.Delay(5000);
            }
        }

        private static async Task PerformHeartbeat()
        {
            var encryptedChallenge = await GetEncryptedChallenge();
            if (string.IsNullOrEmpty(encryptedChallenge))
            {
                LogManager.Log(LogSource.Heartbeat, "Failed to get challenge.", Color.Yellow);
                return;
            }

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
                        LogManager.Log(LogSource.Heartbeat, "Encrypted challenge received.", Color.Gray);
                        return challengeData.EncryptedData;
                    }
                }
                else
                {
                    LogManager.Log(LogSource.Heartbeat, $"Challenge request failed: {response.StatusCode}", Color.Yellow);
                }

                return null;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.Heartbeat, $"Error during challenge request: {ex.Message}", Color.Red);
                return null;
            }
        }

        private static async Task SendChallengeResponse(string encryptedChallenge)
        {
            try
            {
                var decryptedChallenge = SecurityUtil.DecryptHybridChallenge(encryptedChallenge);
                var challenge = JsonSerializer.Deserialize<HeartbeatChallenge>(decryptedChallenge);
                
                if (challenge == null)
                {
                    LogManager.Log(LogSource.Heartbeat, "Challenge decryption failed.", Color.Red);
                    return;
                }

                LogManager.Log(LogSource.Heartbeat, $"Challenge decrypted: {challenge.ChallengeId}", Color.Gray);

                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentTime > challenge.ExpiresAt)
                {
                    LogManager.Log(LogSource.Heartbeat, "Challenge expired.", Color.Yellow);
                    return;
                }

                var responseValue = CalculateChallengeResponse(challenge.ChallengeData);
                
                var response = new HeartbeatResponse
                {
                    ChallengeId = challenge.ChallengeId,
                    ClientId = _clientId,
                    ResponseValue = responseValue,
                    Timestamp = currentTime,
                    Version = VERSION,
                    SystemFingerprint = SecurityUtil.GenerateSystemFingerprint()
                };

                var responseJson = JsonSerializer.Serialize(response);
                var encryptedResponse = SecurityUtil.EncryptResponse(responseJson);

                var encryptedRequest = new EncryptedResponse { EncryptedData = encryptedResponse };
                var json = JsonSerializer.Serialize(encryptedRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponse = await _client.PostAsync($"{SERVER_BASE_URL}/heartbeat_send", content);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseText = await httpResponse.Content.ReadAsStringAsync();
                    LogManager.Log(LogSource.Heartbeat, $"Response sent successfully: {responseText}", Color.Green);
                }
                else
                {
                    LogManager.Log(LogSource.Heartbeat, $"Failed to send response: {httpResponse.StatusCode}", Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.Heartbeat, $"Error processing response: {ex.Message}", Color.Red);
            }
        }

        private static string CalculateChallengeResponse(string challengeData)
        {
            try
            {
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
                LogManager.Log(LogSource.Heartbeat, $"Error calculating challenge response: {ex.Message}", Color.Red);
                return string.Empty;
            }
        }
    }

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