using System;
using System.Diagnostics;
using Lethal_Anti_Cheat.DebugDetector;
using Lethal_Anti_Cheat.Util;

namespace Lethal_Anti_Cheat.DebugDetector
{
    public class RemoteDebuggerCheck : IDebugCheck
    {
        public string MethodName => "CheckRemoteDebuggerPresent";

        public bool IsDebugged(Process process)
        {
            bool isDebugged = false;
            NativeMethods.CheckRemoteDebuggerPresent(process.Handle, ref isDebugged);
            return isDebugged;
        }
    }
}
