using System;
using System.Reactive.Linq;
using System.Threading;
using AdvancedSharpAdbClient.DeviceCommands.Models;
using Avalonia.Threading;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorEffect
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static EventAction<object?> Process(EventAction<object?> action)
    {
        Logger.Info("Processing event " + action.Type);

        if (AppStore.Instance.EmulatorStore.State.IsLoaded)
        {
            return CorAction.Empty;
        }

        // var results = EmulatorScanner.ScanEmulators("Resources/platform-tools/adb.exe", true);
        // Console.WriteLine($"Found {results.Count} emulators");

        var emulatorManager = EmulatorManager.Instance;
        emulatorManager.RefreshDevices();

        Console.WriteLine($"Found {emulatorManager.EmulatorConnections.Count} devices");
        foreach (var emulator in emulatorManager.EmulatorConnections)
        {
            try
            {
                Console.WriteLine(
                    $"Connected to emulator {emulator.DeviceData.Serial} {emulator.DeviceData.Product} {emulator.DeviceData.TransportId}");
                Console.WriteLine("Send shell command");
                var output = emulator.SendShellCommand("getprop ro.product.cpu.abi");
                Console.WriteLine($"Shell Output: {output}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return EmulatorAction.EmulatorConnectSuccessAction.Create(emulatorManager.EmulatorConnections);
    }

    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream.OfAction([EmulatorAction.EmulatorInitAction]).Select(Process);
    }
}