using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lethal_Anti_Debugging.Utils;

namespace Lethal_Anti_Debugging.DebugDetector
{
    public class AppDomainAssemblyCheck : IDebugCheck
    {
        public string MethodName => "AppDomain Assebly Check";

        public bool IsDebugged(Process process)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(asm => asm.FullName.ToLower().Contains("DnSpy") || asm.FullName.ToLower().Contains("debugger"));
        }
    }
}
