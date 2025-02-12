﻿using System;
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

    public void RefreshDevices(bool forceRestart = true)
    {
        adbHelper.InitAdbServer(forceRestart);
        EmulatorConnections.Clear();

        foreach (var emulatorScanData in adbHelper.ConnectByGetDevices())
        {
            EmulatorConnections.Add(
                new EmulatorConnection(emulatorScanData)
            );
        }
    }

    public EmulatorConnection? GetConnection(string id)
    {
        return EmulatorConnections.Find(connection => connection.Id == id);
    }
}