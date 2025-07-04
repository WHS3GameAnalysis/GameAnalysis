namespace Lethal_Anti_Debugging.DebugDetector
{
    public interface IDebugCheck
    {
        bool IsDebugged(System.Diagnostics.Process process);
        string MethodName { get; }
    }
}
