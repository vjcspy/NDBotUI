using System;
using System.Reactive.Linq;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorEffect
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static EventAction Process(EventAction action)
    {
        Logger.Info("Processing event " + action.Type);

        if (AppStore.Instance.EmulatorStore.State.IsLoaded)
        {
            return CoreAction.Empty;
        }

        // var results = EmulatorScanner.ScanEmulators("Resources/platform-tools/adb.exe", true);
        // Console.WriteLine($"Found {results.Count} emulators");
        try
        {
            var emulatorManager = EmulatorManager.Instance;
            emulatorManager.RefreshDevices();

            Console.WriteLine($"Found {emulatorManager.EmulatorConnections.Count} devices");
            foreach (var emulator in emulatorManager.EmulatorConnections)
            {
                try
                {
                    Console.WriteLine(
                        $"Connected to emulator {emulator.DeviceData.Serial} {emulator.DeviceData.Product} {emulator.DeviceData.TransportId}"
                    );
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
        catch (Exception e)
        {
            Logger.Error(e, "Error while processing init ADB");
            return EmulatorAction.EmulatorConnectError.Create();
        }
    }

    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream
            .OfAction(EmulatorAction.EmulatorInitAction, EmulatorAction.EmulatorConnectError)
            .Where(_ => AppStore.Instance.EmulatorStore.State.Attempts < 3)
            .Select(Process);
    }
}