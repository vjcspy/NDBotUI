using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Core.Store;

public partial class GlobalState : ObservableObject
{
    public static GlobalState Instance { get; } = new();

    [ObservableProperty] public string appName = "NDBot";

    [ObservableProperty] public ObservableCollection<EmulatorConnection> emulatorConnections = [];
}