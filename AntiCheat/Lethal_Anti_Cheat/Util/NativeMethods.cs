using System;
using System.Runtime.InteropServices;

namespace Lethal_Anti_Cheat.Util
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileW(string fileName, uint desiredAccess, uint shareMode,
    IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, out int processInformation, int processInformationLength, out int returnLength);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength, out int returnLength);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetLastError();

        [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool mono_is_debugger_attached();
    }
}
