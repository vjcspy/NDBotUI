using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public partial class AppStore : ObservableObject
{
    public static AppStore Instance = new();

    [ObservableProperty] public AppState state = AppState.factory();

    public EmulatorStore EmulatorStore { get; } = EmulatorStore.Instance;

    public MoriStore MoriStore { get; } = MoriStore.Instance;

    public void Reduce(EventAction<object?> action)
    {
        State = AppReducer.Reduce(State, action);
        EmulatorStore.Instance.Reduce(action);
        MoriStore.Instance.Reduce(action);
    }
}