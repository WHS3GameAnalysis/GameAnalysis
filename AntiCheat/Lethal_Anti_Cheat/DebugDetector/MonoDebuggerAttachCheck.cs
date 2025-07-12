using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.DebugDetector
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
