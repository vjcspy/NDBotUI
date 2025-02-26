using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public partial class AppStore : ObservableObject
{
    public static AppStore Instance = new();

    [ObservableProperty] public AppState state = AppState.factory();

    public EmulatorStore EmulatorStore { get; } = EmulatorStore.Instance;

    public MoriStore MoriStore { get; } = MoriStore.Instance;

    public R1999Store R1999Store { get; } = R1999Store.Instance;

    public void Reduce(EventAction action)
    {
        State = AppReducer.Reduce(State, action);
        EmulatorStore.Instance.Reduce(action);
        MoriStore.Instance.Reduce(action);
        R1999Store.Instance.Reduce(action);
    }
}