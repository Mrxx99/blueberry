using Blueberry.Desktop.WindowsApp.Bluetooth;
using System;

namespace Blueberry.Desktop.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var watcher = new DnaBluetoothLEAdvertisementWatcher();

            watcher.StartedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Started listening");
            };

            watcher.StoppedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Stopped listening");
            };

            watcher.NewDeviceDiscoverd += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"New device: {device}");
            };

            watcher.DeviceNameChanged += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Device name changed: {device}");
            };

            watcher.DeviceTimedOut += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device timeout: {device}");
            };

            watcher.StartListening();

            while (true)
            {
                // Pause until we press enter
                Console.ReadLine();

                var devices = watcher.DisocveredDevices;

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{devices.Count} devices ...");

                foreach (var device in devices)
                {
                    Console.WriteLine(device);
                }
            }

            Console.ReadKey();
        }
    }
}
