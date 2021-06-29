using System;
using hid3dxmouse.Api;

namespace hid3dxmouse
{
    internal static class Log
    {
        public static void Error(string error)
        {
            Console.WriteLine(error);
        }

        public static void Win32Error(string message)
        {
            var error = WinApi.GetLastError();
            Console.WriteLine(message);
            Console.WriteLine($"  Error: {error}");
        }

        public static void Win32ErrorIfNot(int code)
        {
            var error = WinApi.GetLastError();
            if (error != code)
                Console.WriteLine($"  Error: {error}");
        }
    }
}
