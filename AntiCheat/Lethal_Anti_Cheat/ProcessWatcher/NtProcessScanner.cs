using Lethal_Anti_Cheat.Util;
using System;
using System.Runtime.InteropServices;

namespace Lethal_Anti_Cheat.ProcessWatcher
{
    public static class NtProcessScanner
    {
        private enum SYSTEM_INFORMATION_CLASS : int
        {
            SystemProcessInformation = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_PROCESS_INFORMATION
        {
            public uint NextEntryOffset;
            public uint NumberOfThreads;
            public long WorkingSetPrivateSize;
            public uint HardFaultCount;
            public uint NumberOfThreadsHighWatermark;
            public ulong CycleTime;
            public long CreateTime;
            public long UserTime;
            public long KernelTime;
            public UNICODE_STRING ImageName;
            public int BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        private static readonly string[] TargetNames = {
            "cheatengine", "dnspy", "x64dbg", "ollydbg", "ida", "ghidra"
        };

        public static void RunOnce()
        {
            Console.WriteLine("\n[NtProcessScanner] NT API Process Scan");

            int bufferSize = 0x10000;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            int status;

            while ((status = NativeMethods.NtQuerySystemInformation(
                        (int)SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                        buffer,
                        bufferSize,
                        out int returnLength)) == -1073741820)
            {
                Marshal.FreeHGlobal(buffer);
                bufferSize *= 2;
                buffer = Marshal.AllocHGlobal(bufferSize);
            }

            IntPtr currentPtr = buffer;

            while (true)
            {
                var procInfo = Marshal.PtrToStructure<SYSTEM_PROCESS_INFORMATION>(currentPtr);

                string processName = "<null>";
                if (procInfo.ImageName.Buffer != IntPtr.Zero)
                {
                    processName = Marshal.PtrToStringUni(procInfo.ImageName.Buffer, procInfo.ImageName.Length / 2);
                }

                int pid = procInfo.UniqueProcessId.ToInt32();

                if (!string.IsNullOrEmpty(processName))
                {
                    string lower = processName.ToLower();
                    foreach (var target in TargetNames)
                    {
                        if (lower.Contains(target))
                        {
                            Console.WriteLine($"  - Detected Process : {processName} (PID: {pid})");
                        }
                    }
                }

                if (procInfo.NextEntryOffset == 0)
                    break;

                currentPtr = IntPtr.Add(currentPtr, (int)procInfo.NextEntryOffset);
            }

            Marshal.FreeHGlobal(buffer);
        }
    }
}
