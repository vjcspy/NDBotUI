using System;
using System.Collections.Generic;
using AdvancedSharpAdbClient;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Typing;

namespace NDBotUI.Modules.Shared.Emulator.Services;

public class EmulatorManager(AdbHelper adbHelper)
{
    public List<EmulatorConnection> EmulatorConnections { get; } = [];

    public void RefreshDevices()
    {
        EmulatorConnections.Clear();

        foreach (var emulatorScanData in adbHelper.ConnectByGetDevices())
        {
            EmulatorConnections.Add(
                new EmulatorConnection(emulatorScanData)
            );
        }
    }
}