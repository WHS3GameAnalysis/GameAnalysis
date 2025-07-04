using System;
using System.Diagnostics;
using Lethal_Anti_Debugging.Utils;

namespace Lethal_Anti_Debugging.DebugDetector
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
