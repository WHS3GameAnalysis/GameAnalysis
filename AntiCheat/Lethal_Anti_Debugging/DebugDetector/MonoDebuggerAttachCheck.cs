using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lethal_Anti_Debugging.Utils;

namespace Lethal_Anti_Debugging.DebugDetector
{
    public class MonoDebuggerAttachCheck : IDebugCheck
    {
        public string MethodName => "Mono Debugger Attach Check";
        public bool IsDebugged(Process _)
        {
            return NativeMethods.mono_is_debugger_attached();
        }
    }
}
