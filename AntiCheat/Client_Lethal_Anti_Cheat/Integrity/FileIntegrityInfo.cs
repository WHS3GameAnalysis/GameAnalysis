namespace LethalAntiCheatLauncher.Integrity
{
    public class FileIntegrityInfo
    {
        public string Filename { get; set; }
        public string Filepath { get; set; }
        public string ExpectedHash { get; set; }
        public string Description { get; set; }
    }
}
