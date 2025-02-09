using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public partial class AppStore : ObservableObject
{
    public static AppStore Instance = new();

    [ObservableProperty] public AppState state = AppState.factory();

    [ObservableProperty] public EmulatorStore emulatorStore = EmulatorStore.Instance;

    public void Reduce(EventAction<object?> action)
    {
        State = AppReducer.Reduce(State, action);
        EmulatorStore.Instance.Reduce(action);
    }

    [ObservableProperty] public ObservableCollection<EmulatorConnection> emulatorConnections = [];
}