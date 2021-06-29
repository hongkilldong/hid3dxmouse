using System;
using System.Runtime.InteropServices;

namespace hid3dxmouse.Api
{
    internal static class SetupApi
    {
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            string enumerator,
            IntPtr hwndParent,
            int flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr deviceInfoSet,
            IntPtr deviceInfoData,
            ref Guid interfaceClassGuid,
            int memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr deviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize,
            ref int requiredSize,
            IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr deviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize,
            ref int requiredSize,
            IntPtr deviceInfoData);


        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public UIntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public UIntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string DevicePath;
        }

        public const int DIGCF_DEFAULT = 0x00000001;
        public const int DIGCF_PRESENT = 0x00000002;
        public const int DIGCF_ALLCLASSES = 0x00000004;
        public const int DIGCF_PROFILE = 0x00000008;
        public const int DIGCF_DEVICEINTERFACE = 0x00000010;
    }

}
