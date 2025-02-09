using System;
using System.Reactive.Linq;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorEffect
{
    private static EventAction<object?> Process(EventAction<object?> action)
    {
        Console.WriteLine("Processing event " + action.Type);

        // var results = EmulatorScanner.ScanEmulators("Resources/platform-tools/adb.exe", true);
        // Console.WriteLine($"Found {results.Count} emulators");

        var adbHelper = new AdbHelper("Resources/platform-tools/adb.exe");

        // Tạo EmulatorManager và refresh danh sách emulator
        var emulatorManager = new EmulatorManager(adbHelper);
        emulatorManager.RefreshDevices();


        foreach (var emulator in emulatorManager.EmulatorConnections)
        {
            try
            {
                Console.WriteLine($"Connected to emulator {emulator.DeviceData.Serial} ({emulator.DeviceType})");

                // Gửi lệnh shell
                Console.WriteLine($"Send shell command");
                var output = emulator.SendShellCommand("getprop ro.product.cpu.abi");
                Console.WriteLine($"Shell Output: {output}");

                // Sử dụng DeviceClient
                var deviceClient = emulator.GetDeviceClient();
                var element = deviceClient.FindElement("//node[@text='Login']");
                if (element != null)
                {
                    Console.WriteLine("Found the Login button!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        return Event.EMPTY_ACTION;
    }

    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream.OfAction([EmulatorAction.EMULATOR_INIT_ACTION]).Select(Process);
    }
}