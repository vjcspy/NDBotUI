using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Core.Store;

public partial class AppStore : ObservableObject
{
    public static AppStore Instance = new();

    [ObservableProperty] public AppState state = AppState.factory();

    public void Reduce(object action)
    {
        State = AppReducer.Reduce(State, action);
        Console.WriteLine($"Count {State.Count}");
    }

    [ObservableProperty] public ObservableCollection<EmulatorConnection> emulatorConnections = [];
}