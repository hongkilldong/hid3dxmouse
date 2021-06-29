using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace hid3dxmouse.Api
{
    internal static class WinApi
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);


        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        public const Int32 ERROR_SUCCESS = 0;
        public const Int32 ERROR_INSUFFICIENT_BUFFER = 122;
        public const Int32 ERROR_NO_MORE_ITEMS = 259;

        public const Int32 FILE_ATTRIBUTE_NORMAL = 0x80;
        public const Int32 FILE_FLAG_OVERLAPPED = 0x40000000;
        public const Int32 FILE_SHARE_READ = 1;
        public const Int32 FILE_SHARE_WRITE = 2;
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;
        public const Int32 OPEN_EXISTING = 3;
    }
}
