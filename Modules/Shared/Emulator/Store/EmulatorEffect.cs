using System;
using System.Reactive.Linq;
using System.Threading;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using Avalonia.Threading;
using NDBotUI.Modules.Core.Store;
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

        var emulatorManager = EmulatorManager.Instance;
        emulatorManager.RefreshDevices();

        Console.WriteLine($"Found {emulatorManager.EmulatorConnections.Count} devices");
        foreach (var emulator in emulatorManager.EmulatorConnections)
        {
            try
            {
                Console.WriteLine($"Connected to emulator {emulator.DeviceData.Serial} ({emulator.DeviceType})");
                Console.WriteLine("Send shell command");
                var output = emulator.SendShellCommand("getprop ro.product.cpu.abi");
                Console.WriteLine($"Shell Output: {output}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        // TODO: move to store
        Dispatcher.UIThread.Post(() =>
        {
            GlobalState.Instance.EmulatorConnections.Clear();
            foreach (var emulator in emulatorManager.EmulatorConnections)
            {
                Console.WriteLine($"Write to state: emulator {emulator.DeviceData.Serial}");
                GlobalState.Instance.EmulatorConnections.Add(emulator);
            }
        });

        return Event.EMPTY_ACTION;
    }

    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream.OfAction([EmulatorAction.EMULATOR_INIT_ACTION]).Select(Process);
    }
}