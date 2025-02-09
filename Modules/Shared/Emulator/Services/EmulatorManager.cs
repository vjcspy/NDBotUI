using System;
using System.Collections.Generic;
using AdvancedSharpAdbClient;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Typing;

namespace NDBotUI.Modules.Shared.Emulator.Services;

public class EmulatorManager(AdbHelper adbHelper)
{
    private readonly AdbHelper _adbHelper = adbHelper;
    public List<EmulatorConnection> EmulatorConnections { get; } = [];

    public void RefreshDevices()
    {
        var adbClient = new AdbClient();
        var devices = adbClient.GetDevices();

        EmulatorConnections.Clear();

        foreach (var device in devices)
        {
            var emulatorConnection = new EmulatorConnection(device.Serial, DetectEmulatorType(device.Model), adbClient);
            emulatorConnection.Connect();
            EmulatorConnections.Add(emulatorConnection);
        }
    }

    public void ReconnectAll()
    {
        foreach (var emulator in EmulatorConnections)
        {
            emulator.Disconnect();
            emulator.Connect();
        }
    }

    public void DisconnectAll()
    {
        foreach (var emulator in EmulatorConnections)
        {
            emulator.Disconnect();
        }
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