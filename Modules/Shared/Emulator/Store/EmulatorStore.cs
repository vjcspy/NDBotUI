using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public partial class EmulatorStore : ObservableObject
{
    public static EmulatorStore Instance = new();

    [ObservableProperty] public EmulatorState state = EmulatorState.factory();

    public void Reduce(EventAction<object?> action)
    {
        State = EmulatorReducer.Reduce(State, action);
    }
}