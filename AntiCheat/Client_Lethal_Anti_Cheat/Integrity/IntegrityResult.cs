using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LethalAntiCheatLauncher.Integrity
{
    public class IntegrityResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        
        // 기존 속성들 (호환성 유지)
        public bool IsValid { get; set; }
        public int TotalFiles { get; set; }
        public int PassedFiles { get; set; }
        public int FailedFilesCount { get; set; }
        public List<string> FailedFiles { get; set; } = new();
        public List<string> SuccessFiles { get; set; } = new();
    }
}
