using System;
using System.Collections.Generic;
using AdvancedSharpAdbClient;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Typing;

namespace NDBotUI.Modules.Shared.Emulator.Services;

public class EmulatorManager(AdbHelper adbHelper)
{
    public static EmulatorManager Instance { get; } = new(new AdbHelper("Resources/platform-tools/adb.exe"));
    public List<EmulatorConnection> EmulatorConnections { get; } = [];

    public void RefreshDevices()
    {
        adbHelper.InitAdbServer();
        EmulatorConnections.Clear();

        foreach (var emulatorScanData in adbHelper.ConnectByGetDevices())
        {
            EmulatorConnections.Add(
                new EmulatorConnection(emulatorScanData)
            );
        }
    }
}