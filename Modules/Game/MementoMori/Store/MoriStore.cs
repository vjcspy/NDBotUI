using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public partial class MoriStore : ObservableObject
{
    public static MoriStore Instance = new();
    
    [ObservableProperty] public MoriState state = MoriState.Factory();

    public void Reduce(EventAction action)
    {
        State = MoriReducer.Reduce(State, action);
    }
}