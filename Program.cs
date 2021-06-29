using System;

namespace hid3dxmouse
{
    class Program
    {
        static void Main(string[] args)
        {
            var mouse = GenericDesktopMultiAxisController.Observe();
            var subsciption = mouse?.Subscribe(input =>
            {
                Console.WriteLine(input.ButtonsPressed.Length > 0
                    ? $"Buttons pressed: {string.Join(", ", input.ButtonsPressed)}"
                    : "Buttons pressed: -");

                Console.WriteLine($"Translate: [{input.T.x}, {input.T.y}, {input.T.z}]");
                Console.WriteLine($"Rotate:    [{input.R.x}, {input.R.y}, {input.R.z}]");
            });

            Console.ReadKey(true);

            subsciption?.Dispose();
        }
    }
}
