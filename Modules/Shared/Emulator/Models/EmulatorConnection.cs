using System;
using System.Linq;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using NDBotUI.Modules.Shared.Emulator.Errors;
using NDBotUI.Modules.Shared.Emulator.Typing;

namespace NDBotUI.Modules.Shared.Emulator.Models;

public class EmulatorConnection(EmulatorScanData emulatorScanData)
{
    public DeviceData DeviceData { get; } = emulatorScanData.DeviceData;
    private DeviceClient? _deviceClient;
    public string DeviceType => DetectEmulatorType(emulatorScanData.DeviceData.Model);

    public string SendShellCommand(string command)
    {
        var receiver = new ConsoleOutputReceiver();
        emulatorScanData.AdbClient.ExecuteRemoteCommand(command, emulatorScanData.DeviceData, receiver);

        return receiver.ToString();
    }

    public DeviceClient GetDeviceClient()
    {
        if (_deviceClient == null)
        {
            _deviceClient = new DeviceClient(emulatorScanData.AdbClient, emulatorScanData.DeviceData);
        }

        return _deviceClient;
    }

    private string DetectEmulatorType(string model)
    {
        if (model.Contains(EmulatorTypes.Bluestacks, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.Bluestacks;
        if (model.Contains(EmulatorTypes.Nox, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.Nox;
        if (model.Contains(EmulatorTypes.LDPlayer, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.LDPlayer;

        return "Unknown";
    }
}