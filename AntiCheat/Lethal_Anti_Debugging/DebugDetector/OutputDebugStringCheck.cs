using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lethal_Anti_Debugging.Utils;

namespace Lethal_Anti_Debugging.DebugDetector
{
    public class OutputDebugStringCheck : IDebugCheck
    {
        public string MethodName => "OutputDebugString";

        public bool IsDebugged(Process process)
        {
            uint before = NativeMethods.GetLastError();
            NativeMethods.OutputDebugString("Debugging check message");
            uint after = NativeMethods.GetLastError();
            return (before != after);
        }

    }
}
