using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Helpers;
using NDBotUI.Modules.Shared.Emulator.Models;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Services;

public class EmulatorManager(AdbHelper adbHelper)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static EmulatorManager Instance { get; } = new(new AdbHelper("Resources/platform-tools/adb.exe"));
    public Lst<EmulatorConnection> EmulatorConnections { get; set; } = [];

    public void RefreshDevices(bool forceRestart = true, bool reinit = true)
    {
        if (reinit) adbHelper.InitAdbServer(forceRestart);

        Lst<EmulatorConnection> newConnections = [];

        var devices = adbHelper.ConnectByGetDevices();
        Logger.Info("Found {0} devices", devices.Count());
        
        foreach (var emulatorScanData in devices)
            newConnections = newConnections.Add(
                new EmulatorConnection(emulatorScanData)
            );

        var list1Keys = newConnections.Map(e => e.Id).ToHashSet();
        var list2Keys = EmulatorConnections.Map(e => e.Id).ToHashSet();

        var isEqual = list1Keys.SetEquals(list2Keys);

        if (!isEqual) EmulatorConnections = newConnections;
    }

    public EmulatorConnection? GetConnection(string id)
    {
        return EmulatorConnections.Find(connection => connection.Id == id).Match(x => x, () => null!);
    }
}