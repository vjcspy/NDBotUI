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

        var adbHelper = new AdbHelper("Resources/adb/adb.exe");

        // Tạo EmulatorManager và refresh danh sách emulator
        var emulatorManager = new EmulatorManager(adbHelper);
        emulatorManager.RefreshDevices();

        try
        {
            foreach (var emulator in emulatorManager.EmulatorConnections)
            {
                Console.WriteLine($"Connected to emulator {emulator.Id} ({emulator.EmulatorType})");

                // Gửi lệnh shell
                var output = emulator.SendShellCommand("ls");
                Console.WriteLine($"Shell Output: {output}");

                // Sử dụng DeviceClient
                var deviceClient = emulator.GetDeviceClient();
                var element = deviceClient.FindElement("//node[@text='Login']");
                if (element != null)
                {
                    Console.WriteLine("Found the Login button!");
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }


        return Event.EMPTY_ACTION;
    }

    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream.OfAction([EmulatorAction.EMULATOR_INIT_ACTION]).Select(Process);
    }
}