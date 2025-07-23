using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ReflectionTest
{
    public class ConsoleManager
    {
        const uint GENERIC_WRITE = 0x40000000;
        const uint GENERIC_READ = 0x80000000;
        const uint FILE_SHARE_READ = 1;
        const uint FILE_SHARE_WRITE = 2;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateFileW(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        public static void SetupIn() => Console.SetIn(CreateInStream());
        public static void SetupOut() => Console.SetOut(CreateOutStream());

        private static StreamWriter CreateOutStream() => new(CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write)) { AutoFlush = true };
        private static StreamReader CreateInStream() => new(CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read));

        private static FileStream CreateFileStream(string name, uint access, uint share, FileAccess fileAccess)
        {
            IntPtr handle = CreateFileW(name, access, share, IntPtr.Zero, (uint)FileMode.Open, (uint)FileAttributes.Normal, IntPtr.Zero);
            var safeHandle = new SafeFileHandle(handle, true);
            return !safeHandle.IsInvalid ? new FileStream(safeHandle, fileAccess) : null;
        }
    }
}
