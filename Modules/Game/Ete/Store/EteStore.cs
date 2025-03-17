using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store;

public partial class EteStore : ObservableObject
{
    public static EteStore Instance = new();

    [ObservableProperty] public EteState state = EteState.Factory();

    public void Reduce(EventAction action)
    {
        State = EteReducer.Reduce(State, action);
    }
}