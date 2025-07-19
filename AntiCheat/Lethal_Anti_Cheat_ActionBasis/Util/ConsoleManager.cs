using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Lethal_Anti_Cheat.Util
{
    public static class ConsoleManager
    {
        const uint GENERIC_WRITE = 0x40000000;
        const uint GENERIC_READ = 0x80000000;
        const uint FILE_SHARE_READ = 1;
        const uint FILE_SHARE_WRITE = 2;
        public static void Initialize()
        {
            NativeMethods.AllocConsole();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.SetOut(new StreamWriter(CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write)) { AutoFlush = true });
            Console.SetIn(new StreamReader(CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read)));
        }

        private static FileStream CreateFileStream(string name, uint access, uint share, FileAccess fileAccess)
        {
            IntPtr handle = NativeMethods.CreateFileW(name, access, share, IntPtr.Zero, (uint)FileMode.Open,
                (uint)FileAttributes.Normal, IntPtr.Zero);
            var safeHandle = new SafeFileHandle(handle, true);
            return new FileStream(safeHandle, fileAccess);
        }
    }
}
