using System.Collections.Generic;

namespace LethalAntiCheatLauncher.Integrity
{
    public class IntegrityResult
    {
        public bool IsValid { get; set; }
        public int TotalFiles { get; set; }
        public int PassedFiles { get; set; }
        public int FailedFilesCount { get; set; }
        public string Message { get; set; }
        public List<string> FailedFiles { get; set; } = new();
        public List<string> SuccessFiles { get; set; } = new();
    }
}
