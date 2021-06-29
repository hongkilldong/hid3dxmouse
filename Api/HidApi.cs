using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace hid3dxmouse.Api
{
    internal static class HidApi
    {
        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(ref Guid guid);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(SafeFileHandle hidDeviceObject, ref IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(SafeFileHandle hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(SafeFileHandle hidDeviceObject, StringBuilder sb, int bufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetProductString(SafeFileHandle hidDeviceObject, StringBuilder sb, int bufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetSerialNumberString(SafeFileHandle hidDeviceObject, StringBuilder sb, int bufferLength);

        [DllImport("hid.dll")]
        public static extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS caps);

        [DllImport("hid.dll")]
        public static extern int HidP_MaxUsageListLength(HIDP_REPORT_TYPE reportType, ushort usagePage, IntPtr preparsedData);

        [DllImport("hid.dll")]
        public static extern int HidP_GetUsages(
            HIDP_REPORT_TYPE reportType, 
            ushort usagePage, 
            ushort linkCollection, 
            IntPtr usageList, 
            ref int usageLength, 
            IntPtr preparsedData, 
            IntPtr report, 
            int reportLength);

        [DllImport("hid.dll")]
        public static extern int HidP_GetScaledUsageValue(
            HIDP_REPORT_TYPE reportType,
            ushort usagePage,
            ushort linkCollection,
            ushort usage,
            ref int usageValue,
            IntPtr preparsedData,
            IntPtr report,
            int reportLength);

        [DllImport("hid.dll")]
        public static extern int HidP_GetUsageValue(
            HIDP_REPORT_TYPE reportType,
            ushort usagePage,
            ushort linkCollection,
            ushort usage,
            ref int usageValue,
            IntPtr preparsedData,
            IntPtr report,
            int reportLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17, ArraySubType = UnmanagedType.U2)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        public const int HIDP_STATUS_SUCCESS = (0x11 << 16) | 0x0;
        public const int HIDP_STATUS_INCOMPATIBLE_REPORT_ID = (0xC << 28) | (0x11 << 16) | 0xA;

        public enum HIDP_REPORT_TYPE
        {
            HidP_Input,
            HidP_Output,
            HidP_Feature,
        }

        public const ushort HID_USAGE_PAGE_GENERIC = 0x01;
        public const ushort HID_USAGE_PAGE_BUTTON = 0x09;

        public const ushort HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER = 0x08;

        public const ushort HID_USAGE_GENERIC_X = 0x30;
        public const ushort HID_USAGE_GENERIC_Y = 0x31;
        public const ushort HID_USAGE_GENERIC_Z = 0x32;
        public const ushort HID_USAGE_GENERIC_RX = 0x33;
        public const ushort HID_USAGE_GENERIC_RY = 0x34;
        public const ushort HID_USAGE_GENERIC_RZ = 0x35;
    }
}
