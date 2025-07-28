using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Drawing;
using LethalAntiCheatLauncher.Util; // HwidUtil, SecurityUtil을 위한 using 추가

namespace LethalAntiCheatLauncher.Integrity
{
    public static class ServerHashManager
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string SERVER_BASE_URL = "https://ghb.r-e.kr";
        
        public static async Task<IntegrityResult> CheckIntegrityWithServerAsync()
        {
            try
            {
                LogManager.Log(LogSource.Integrity, "무결성 검사 시작", Color.Cyan);
                
                // 1. 로컬 해시값 추출
                LogManager.Log(LogSource.Integrity, "로컬 파일 해시 추출 중...", Color.LightBlue);
                var localHashes = ExtractLocalHashes();
                LogManager.Log(LogSource.Integrity, $"해시 추출 완료 ({localHashes.Count}개 파일)", Color.LightGreen);
                
                // 2. 서버로 전송할 데이터 준비
                var requestData = new IntegrityRequest
                {
                    ClientId = HwidUtil.GetHwid(),
                    GameVersion = "v72", // 나중에 동적으로 가져오기
                    Hashes = localHashes,
                    Timestamp = DateTime.UtcNow
                };
                
                // 3. 암호화하여 서버로 전송
                LogManager.Log(LogSource.Integrity, "서버측 무결성 검증 중...", Color.LightBlue);
                var encryptedRequest = SecurityUtil.EncryptResponse(JsonSerializer.Serialize(requestData));
                var response = await _client.PostAsync($"{SERVER_BASE_URL}/api/integrity/check", 
                    new StringContent(JsonSerializer.Serialize(new { encrypted_data = encryptedRequest }), Encoding.UTF8, "application/json"));
                
                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<EncryptedResponse>(responseText);
                    
                    // 4. 응답 복호화
                    var decryptedResponse = SecurityUtil.DecryptHybridChallenge(responseData.EncryptedData);
                    var result = JsonSerializer.Deserialize<IntegrityResult>(decryptedResponse);
                    
                    // Status를 기반으로 IsValid 설정
                    result.IsValid = result.Status == "success";
                    
                    // 5. 개별 파일 검증 결과 로그 출력
                    LogIndividualFileResults(result);
                    
                    LogManager.Log(LogSource.Integrity, $"검사 결과: {result.Message}", result.IsValid ? Color.Green : Color.Red);
                    return result;
                }
                else
                {
                    LogManager.Log(LogSource.Integrity, $"서버 연결 실패 ({response.StatusCode})", Color.Red);
                    return new IntegrityResult { IsValid = false, Message = "서버 통신 실패" };
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogSource.Integrity, $"검사 오류: {ex.Message}", Color.Red);
                return new IntegrityResult { IsValid = false, Message = $"오류: {ex.Message}" };
            }
        }
        
        private static List<FileHashInfo> ExtractLocalHashes()
        {
            var hashes = new List<FileHashInfo>();
            var gamePath = FindLethalCompanyPath();
            
            if (string.IsNullOrEmpty(gamePath))
            {
                LogManager.Log(LogSource.Integrity, "게임 경로를 찾을 수 없음", Color.Red);
                return hashes;
            }
            
            string[] filenames = {
                "Lethal Company.exe",
                "Lethal Company_Data/Managed/Assembly-CSharp.dll",
                "UnityPlayer.dll",
                "Lethal Company_Data/globalgamemanagers",
                "Lethal Company_Data/Managed/UnityEngine.CoreModule.dll",
                "Lethal Company_Data/Managed/Unity.Netcode.Runtime.dll",
                "Lethal Company_Data/Managed/Unity.InputSystem.dll",
                "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll",
                "Lethal Company_Data/Plugins/x86_64/steam_api64.dll",
                "Lethal Company_Data/Managed/UnityEngine.dll"
            };
            
            for (int i = 0; i < filenames.Length; i++)
            {
                var filename = filenames[i];
                var filepath = Path.Combine(gamePath, filename.Replace("/", "\\"));
                var fileNameOnly = Path.GetFileName(filename);
                
                if (File.Exists(filepath))
                {
                    var hash = FileHashUtil.CalculateSHA256(filepath);
                    if (!string.IsNullOrEmpty(hash))
                    {
                        hashes.Add(new FileHashInfo
                        {
                            Filename = fileNameOnly,
                            Hash = hash
                        });
                        LogManager.Log(LogSource.Integrity, $"✓ {fileNameOnly} ({i + 1}/{filenames.Length})", Color.LightGreen);
                    }
                    else
                    {
                        LogManager.Log(LogSource.Integrity, $"✗ {fileNameOnly} ({i + 1}/{filenames.Length})", Color.Yellow);
                    }
                }
                else
                {
                    LogManager.Log(LogSource.Integrity, $"? {fileNameOnly} ({i + 1}/{filenames.Length})", Color.Yellow);
                }
            }
            
            return hashes;
        }
        
        private static void LogIndividualFileResults(IntegrityResult result)
        {
            // 성공한 파일들 로그 출력
            if (result.SuccessFiles != null && result.SuccessFiles.Count > 0)
            {
                foreach (var filename in result.SuccessFiles)
                {
                    LogManager.Log(LogSource.Integrity, $"{filename} 검증 완료", Color.LightGreen);
                }
            }
            
            // 실패한 파일들 로그 출력
            if (result.FailedFiles != null && result.FailedFiles.Count > 0)
            {
                foreach (var filename in result.FailedFiles)
                {
                    LogManager.Log(LogSource.Integrity, $"{filename} 검증 실패", Color.Red);
                }
            }
        }
        
        private static string FindLethalCompanyPath()
        {
            // 기존 IntegrityChecker의 경로 찾기 로직 활용
            string[] paths = {
                @"C:\Program Files (x86)\Steam\steamapps\common\Lethal Company",
                @"C:\Steam\steamapps\common\Lethal Company",
                @"D:\Steam\steamapps\common\Lethal Company",
                @"D:\Games\Steam\steamapps\common\Lethal Company",
                @"E:\Steam\steamapps\common\Lethal Company",
                @"C:\SteamLibrary\steamapps\common\Lethal Company"
            };
            
            return paths.FirstOrDefault(p => File.Exists(Path.Combine(p, "Lethal Company.exe"))) ?? "";
        }
    }
    
    public class IntegrityRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "";
        [JsonPropertyName("game_version")]
        public string GameVersion { get; set; } = "";
        [JsonPropertyName("hashes")]
        public List<FileHashInfo> Hashes { get; set; } = new();
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
    
    public class FileHashInfo
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; } = "";
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = "";
    }
    
    public class EncryptedResponse
    {
        [JsonPropertyName("encrypted_data")]
        public string EncryptedData { get; set; } = "";
    }
} 