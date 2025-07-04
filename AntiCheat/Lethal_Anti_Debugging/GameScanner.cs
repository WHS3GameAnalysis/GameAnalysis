using System.Diagnostics;

namespace Lethal_Anti_Debugging
{
    public static class GameScanner
    {
        public static Process? FindGame(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return null; // Game not found
            }
            return processes[0]; // Return the first instance of the game
        }
    }
}
