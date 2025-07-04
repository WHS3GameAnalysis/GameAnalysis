using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lethal_Anti_Debugging.DebugDetector;
using Lethal_Anti_Debugging.Utils;

namespace Lethal_Anti_Debugging.DebugDetector
{
    public class NtQueryCheck : IDebugCheck
    {
        public string MethodName => "NtQueryInformationProcess (DebugFlags)";

        private const int ProcessDebugFlags = 0x1F;

        public bool IsDebugged(Process process)
        {
            int debugFlag;
            int returnLength;

            int status = NativeMethods.NtQueryInformationProcess(
                process.Handle,
                ProcessDebugFlags,
                out debugFlag,
                sizeof(int),
                out returnLength);
            return (status == 0 && debugFlag == 0);
        }

    }
}
