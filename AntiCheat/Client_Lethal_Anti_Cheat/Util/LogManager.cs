using System;
using System.Drawing;

namespace LethalAntiCheatLauncher.Util
{
    public enum LogSource
    {
        All,
        AntiCheat,
        Integrity,
        Heartbeat,
        SimpleAC,
        DLL,
        Behavior,
        Debug,
        Harmony,
        Process,
        Reflection
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; }
        public LogSource Source { get; }
        public string Message { get; }
        public Color Color { get; }

        public LogEntry(LogSource source, string message, Color color)
        {
            Timestamp = DateTime.Now;
            Source = source;
            Message = message;
            Color = color;
        }
    }

    public static class LogManager
    {
        public static event Action<LogEntry> OnLogReceived;

        public static void Log(LogSource source, string message, Color color)
        {
            OnLogReceived?.Invoke(new LogEntry(source, message, color));
        }
    }
}