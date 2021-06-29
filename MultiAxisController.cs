using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using hid3dxmouse.Api;

namespace hid3dxmouse
{
    public enum ValueInputError
    {
        NoError,
        Tx,
        Ty,
        Tz,
        Rx,
        Ry,
        Rz
    }

    public readonly struct Input
    {
        public int[] ButtonsPressed { get; }
        public (int x, int y, int z) T { get; }
        public (int x, int y, int z) R { get; }
        public (ValueInputError error, int code) Error { get; }
        public Input(int[] buttonsPressed, (int x, int y, int z) t, (int x, int y, int z) r, (ValueInputError error, int code) error)
        {
            ButtonsPressed = buttonsPressed;
            T = t;
            R = r;
            Error = error;
        }
    }

    internal class ControllerInputReader : IDisposable
    {
        private readonly IObserver<Input> observer;
        private readonly HidDevice hidDevice;
        private readonly Thread thread;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ControllerInputReader(HidDevice hidDevice, IObserver<Input> observer)
        {
            this.hidDevice = hidDevice;
            this.observer = observer;
            thread = new Thread(Read);
            thread.Start(cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            thread.Join(TimeSpan.FromSeconds(1));
            hidDevice.Dispose();
        }

        private async void Read(object param)
        {
            using var deviceFile = WinApi.CreateFile(hidDevice.DevicePath, 
                WinApi.GENERIC_READ| WinApi.GENERIC_WRITE,
                WinApi.FILE_SHARE_READ | WinApi.FILE_SHARE_WRITE, 
                IntPtr.Zero, 
                WinApi.OPEN_EXISTING,
                WinApi.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (deviceFile.IsInvalid)
            {
                Log.Win32Error("Failed to open device as file");
                observer.OnCompleted();
                return;
            }

            using var deviceFileStream = new FileStream(deviceFile, FileAccess.ReadWrite);

            var inputBuffer = new byte[hidDevice.InputReportLength];

            var token = (CancellationToken)param;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var task = deviceFileStream.ReadAsync(inputBuffer, 0, inputBuffer.Length, token);
                    var cb = await task;

                    if (cb > 0)
                    {
                        var buttonsPressed = hidDevice.GetButtonsPressed(inputBuffer);

                        var tx = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_X, inputBuffer);
                        if (tx.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, 0, 0), (0, 0, 0), 
                                (ValueInputError.Tx, tx.status)));
                            continue; 
                        }

                        var ty = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_Y, inputBuffer);
                        if (ty.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, ty.value, 0), (0, 0, 0),
                                (ValueInputError.Ty, ty.status)));
                            continue;
                        }

                        var tz = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_Z, inputBuffer);
                        if (tz.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, ty.value, tz.value), (0, 0, 0), 
                                (ValueInputError.Tz, tz.status)));
                            continue;
                        }

                        var rx = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_RX, inputBuffer);
                        if (rx.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, ty.value, tz.value), (rx.value, 0, 0),
                                (ValueInputError.Rx, rx.status)));
                            continue;
                        }

                        var ry = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_RY, inputBuffer);
                        if (ry.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, ty.value, tz.value), (rx.value, ry.value, 0),
                                (ValueInputError.Ry, ry.status)));
                            continue;
                        }

                        var rz = hidDevice.GetUsageValue(HidApi.HID_USAGE_GENERIC_RZ, inputBuffer);
                        if (rz.status != 0)
                        {
                            observer.OnNext(new Input(buttonsPressed, (tx.value, ty.value, tz.value), (rx.value, ry.value, rz.value),
                                (ValueInputError.Rz, rz.status)));
                            continue;
                        }

                        observer.OnNext(new Input(buttonsPressed, 
                            (tx.value, ty.value, tz.value), 
                            (rx.value, ry.value, rz.value), 
                            (ValueInputError.NoError, 0)));
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    observer.OnCompleted();
                }
            }

            observer.OnCompleted();
        }
    }

    public class GenericDesktopMultiAxisController
    {
        public static IObservable<Input> Observe()
        {
            Console.Write("Detecting controller...");

            var device = HidDevice.SelectDevice(x => x.UsagePage == HidApi.HID_USAGE_PAGE_GENERIC &&
                                                    x.Usage == HidApi.HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER)
                .FirstOrDefault();

            if (null == device) 
                return Observable.Empty<Input>();

            Console.WriteLine($"{device.Product}, {device.Manufacturer}");
            return Observable.Create<Input>(observer => new ControllerInputReader(device, observer));
        }
    }
}
