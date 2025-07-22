using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LethalAntiCheatLauncher.Util
{
    public static class HeartbeatManager
    {
        private static bool _started = false;
        public static void Start()
        {
            if (_started) return;
            _started = true;
            _ = SendHeartbeatLoop();
        }

        private static async Task SendHeartbeatLoop()
        {
            var client = new HttpClient();
            const string serverUrl = "https://ghb.r-e.kr/heartbeat";
            const string clientId = "test123";
            const string version = "1.0.0";

            while (true)
            {
                try
                {
                    var json = $"{{\"client_id\":\"{clientId}\",\"timestamp\":\"{DateTime.UtcNow:o}\",\"version\":\"{version}\"}}";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(serverUrl, content);
                    var resp = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Heartbeat] 서버 응답: {resp}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Heartbeat] 전송 실패: {ex.Message}");
                }
                await Task.Delay(5000);
            }
        }
    }
}