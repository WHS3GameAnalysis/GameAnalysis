using Lethal_Anti_Debugging.DebugDetector;
using System;

class Program
{
    static void Main()
    {
        string gameName = "Lethal Company"; 
        var gameProcess = Lethal_Anti_Debugging.GameScanner.FindGame(gameName);

        if(gameProcess == null)
        {
            Console.WriteLine($"Game '{gameName}' not found.");
            return;
        }
        else
        {
            Console.WriteLine($"Game '{gameName}' found with Process ID: {gameProcess.Id}");
        }

        var checkers = new List<IDebugCheck>
        {
            new RemoteDebuggerCheck(),
            new NtQueryCheck(),
            new OutputDebugStringCheck(),
            new AppDomainAssemblyCheck(), // Assuming you have an AppDomainCheck class
            new MonoPortScanCheck(), // Assuming you have a MonoPortScanCheck class
            new MonoDebuggerAttachCheck(), // Assuming you have a MonoDebuggerAttachCheck class
            // You can add more checks here if needed
        };

        foreach (var checker in checkers)
        {
            bool isDebugged = checker.IsDebugged(gameProcess);
            Console.WriteLine($"Using {checker.MethodName}: Is the game '{gameName}' being debugged? {isDebugged}");
        }
    }
}