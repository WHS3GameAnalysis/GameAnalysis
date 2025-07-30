using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using LethalAntiCheatLauncher.Util;

namespace LethalAntiCheatLauncher.Util
{
    public static class BehaviorLogManager
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string SERVER_URL = "https://ghb.r-e.kr";

        public static void SendLog(string message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var data = new { message = message };
                    var json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await _client.PostAsync($"{SERVER_URL}/api/behavior/log", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        LogManager.Log(LogSource.Behavior, $"서버 로그 전송 성공", Color.Green);
                    }
                    else
                    {
                        LogManager.Log(LogSource.Behavior, $"서버 로그 전송 실패: {response.StatusCode}", Color.Yellow);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogSource.Behavior, $"서버 로그 전송 오류: {ex.Message}", Color.Red);
                }
            });
        }
    }
} 