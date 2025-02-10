using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public partial class AppStore : ObservableObject
{
    public static AppStore Instance = new();

    [ObservableProperty] public AppState state = AppState.factory();

    [ObservableProperty] public EmulatorStore emulatorStore = EmulatorStore.Instance;
    [ObservableProperty] public MoriStore moriStore = MoriStore.Instance;

    public void Reduce(EventAction<object?> action)
    {
        State = AppReducer.Reduce(State, action);
        EmulatorStore.Instance.Reduce(action);
        MoriStore.Instance.Reduce(action);
    }
}