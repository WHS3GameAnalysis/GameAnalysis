using System;
using System.Collections.Generic;
using System.Text;

namespace Lethal_Anti_Cheat.DebugDetector
{
    public interface IDebugCheck
    {
        bool IsDebugged(System.Diagnostics.Process process);
        string MethodName { get; }
    }
}
