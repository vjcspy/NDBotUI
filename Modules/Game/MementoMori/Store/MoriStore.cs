using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public partial class MoriStore : ObservableObject
{
    public static MoriStore Instance = new();
    
    [ObservableProperty] public MoriState state = MoriState.factory();

    public void Reduce(EventAction<object?> action)
    {
        State = MoriReducer.Reduce(State, action);
    }
}