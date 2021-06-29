using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using hid3dxmouse.Api;

namespace hid3dxmouse
{
    public class HidDevice : IDisposable
    {
        private IntPtr hppd;
        private HidApi.HIDP_CAPS caps;

        private HidDevice()
        {
        }

        public void Dispose()
        {
            HidApi.HidD_FreePreparsedData(hppd);
        }

        public override string ToString()
        {
            return $"UsagePage:{UsagePage:X},Usage:{Usage:X},{Product},{Manufacturer}";
        }

        public string DevicePath { get; private set; }
        public int VendorId { get; private set; }
        public int ProductId { get; private set; }
        public int VersionNumber { get; internal set; }
        public int UsagePage => caps.UsagePage;
        public int Usage => caps.Usage;
        public string Manufacturer { get; private set; }
        public string SerialNumber { get; private set; }
        public string Product { get; private set; }
        public int Buttons { get; private set; }

        public int[] GetButtonsPressed(byte[] report)
        {
            var buttonsPressed = new ushort[Buttons];
            var buttons = Buttons;

            var reportBufferHandle = GCHandle.Alloc(report, GCHandleType.Pinned);
            var buttonsPressedHandle = GCHandle.Alloc(buttonsPressed, GCHandleType.Pinned);
            var buttonsPressedHandlePtr = buttonsPressedHandle.AddrOfPinnedObject();

            try
            {
                if (HidApi.HIDP_STATUS_SUCCESS !=
                    HidApi.HidP_GetUsages(
                        HidApi.HIDP_REPORT_TYPE.HidP_Input,
                        HidApi.HID_USAGE_PAGE_BUTTON,
                        0,
                        buttonsPressedHandlePtr,
                        ref buttons,
                        hppd,
                        reportBufferHandle.AddrOfPinnedObject(),
                        report.Length))
                {
                    return new int[0];
                }

                return buttonsPressed.Take(buttons).Select(x => (int)x).ToArray();
            }
            finally
            {
                reportBufferHandle.Free();
                buttonsPressedHandle.Free();
            }
        }

        public (int value, int status) GetUsageValue(ushort usage, byte[] report)
        {
            var usageValue = 0;
            var reportBufferHandle = GCHandle.Alloc(report, GCHandleType.Pinned);

            const int c = 0x8000;

            try
            {
                
                var status = HidApi.HidP_GetUsageValue(
                    HidApi.HIDP_REPORT_TYPE.HidP_Input,
                    HidApi.HID_USAGE_PAGE_GENERIC,
                    0,
                    usage,
                    ref usageValue,
                    hppd,
                    reportBufferHandle.AddrOfPinnedObject(),
                    report.Length);

                switch (status)
                {
                    case HidApi.HIDP_STATUS_SUCCESS:
                    case HidApi.HIDP_STATUS_INCOMPATIBLE_REPORT_ID:
                        return (usageValue & c) == c ? ((c ^ usageValue) - c, 0) : (usageValue, 0);
                    default:
                        Log.Error($"Error while getting usage {usage}: {status:X}");
                        break;
                }

                return (usageValue, status);
            }
            finally
            {
                reportBufferHandle.Free();
            }
        }

        public ushort InputReportLength => caps.InputReportByteLength;

        public static List<HidDevice> SelectDevice(Func<HidDevice, bool> filter)
        {
            var devices = new List<HidDevice>();

            var hidGuid = new Guid();
            HidApi.HidD_GetHidGuid(ref hidGuid);

            var deviceInfoSet = SetupApi.SetupDiGetClassDevs(ref hidGuid, null, IntPtr.Zero,
                SetupApi.DIGCF_PRESENT|SetupApi.DIGCF_DEVICEINTERFACE);

            if (WinApi.INVALID_HANDLE_VALUE == deviceInfoSet)
            {
                Log.Win32Error("Failed to get device information set");
                return devices;
            }

            var deviceInterfaceData = new SetupApi.SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf<SetupApi.SP_DEVICE_INTERFACE_DATA>() };

            var moreDevices = true;
            var index = 0;

            while (moreDevices)
            {
                if (SetupApi.SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref hidGuid, index++, ref deviceInterfaceData))
                {
                    var cb = 0;
                    SetupApi.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, 
                        ref deviceInterfaceData, IntPtr.Zero, 0, ref cb,
                        IntPtr.Zero);

                    Log.Win32ErrorIfNot(WinApi.ERROR_INSUFFICIENT_BUFFER);

                    var deviceInterfaceDetailData = new SetupApi.SP_DEVICE_INTERFACE_DETAIL_DATA
                        {cbSize = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8};

                    if (SetupApi.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, 
                        ref deviceInterfaceData, ref deviceInterfaceDetailData, cb, ref cb,
                        IntPtr.Zero))
                    {
                        var device = GetDeviceDescriptor(deviceInterfaceDetailData.DevicePath);
                        if (null == device) 
                            continue;

                        if (filter(device))
                            devices.Add(device);
                        else
                            device.Dispose();
                    }
                    else
                    {
                        Log.Win32ErrorIfNot(WinApi.ERROR_SUCCESS);
                    }
                }
                else
                {
                    Log.Win32ErrorIfNot(WinApi.ERROR_NO_MORE_ITEMS);
                    moreDevices = false;
                }
            }

            SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return devices;
        }

        private static HidDevice GetDeviceDescriptor(string devicePath)
        {
            var hidDevice = new HidDevice { DevicePath = devicePath };

            using var deviceHandle = WinApi.CreateFile(devicePath,
                0,
                WinApi.FILE_SHARE_READ | WinApi.FILE_SHARE_WRITE,
                IntPtr.Zero,
                WinApi.OPEN_EXISTING,
                WinApi.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (deviceHandle.IsInvalid)
            {
                Log.Win32Error($"Failed to open device {devicePath}");
                return null;
            }

            if (!HidApi.HidD_GetPreparsedData(deviceHandle, ref hidDevice.hppd))
            {
                Log.Win32Error($"Failed to get pre-parsed data for device {devicePath}");
                return null;
            }

            var attr = new HidApi.HIDD_ATTRIBUTES { Size = Marshal.SizeOf<HidApi.HIDD_ATTRIBUTES>() };
            if (!HidApi.HidD_GetAttributes(deviceHandle, ref attr))
            {
                Log.Win32Error($"Failed to get attributes of device {devicePath}");
                return null;
            }

            hidDevice.VendorId = attr.VendorID;
            hidDevice.ProductId = attr.ProductID;
            hidDevice.VersionNumber = attr.VersionNumber;

            if (HidApi.HIDP_STATUS_SUCCESS != HidApi.HidP_GetCaps(hidDevice.hppd, ref hidDevice.caps))
            {
                Log.Error($"Failed to get capabilities of device {devicePath}");
                return null;
            }

            var sb = new StringBuilder(1000);
            HidApi.HidD_GetManufacturerString(deviceHandle, sb, sb.Capacity + 1);
            hidDevice.Manufacturer = sb.ToString();
            sb.Clear();

            HidApi.HidD_GetProductString(deviceHandle, sb, sb.Capacity + 1);
            hidDevice.Product = sb.ToString();
            sb.Clear();

            HidApi.HidD_GetSerialNumberString(deviceHandle, sb, sb.Capacity + 1);
            hidDevice.SerialNumber = sb.ToString();

            hidDevice.Buttons = HidApi.HidP_MaxUsageListLength(HidApi.HIDP_REPORT_TYPE.HidP_Input, HidApi.HID_USAGE_PAGE_BUTTON, hidDevice.hppd);

            return hidDevice;
        }
    }
}
