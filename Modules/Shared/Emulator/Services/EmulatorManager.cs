using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Shared.Emulator.Services;

public class EmulatorManager(AdbHelper adbHelper)
{
    public static EmulatorManager Instance { get; } = new(new AdbHelper("Resources/platform-tools/adb.exe"));
    public Lst<EmulatorConnection> EmulatorConnections { get; set; } = [];

    public void RefreshDevices(bool forceRestart = true, bool reinit = true)
    {
        if (reinit) adbHelper.InitAdbServer(forceRestart);

        EmulatorConnections = [];

        foreach (var emulatorScanData in adbHelper.ConnectByGetDevices())
            EmulatorConnections = EmulatorConnections.Add(
                new EmulatorConnection(emulatorScanData)
            );
    }

    public EmulatorConnection? GetConnection(string id)
    {
        return EmulatorConnections.Find(connection => connection.Id == id).Match(x => x, () => null!);
    }
}