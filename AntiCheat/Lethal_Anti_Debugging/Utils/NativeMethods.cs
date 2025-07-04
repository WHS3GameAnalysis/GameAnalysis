using System;
using System.Runtime.InteropServices;

namespace Lethal_Anti_Debugging.Utils
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, out int processInformation, int processInformationLength, out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetLastError();


    }
}
