using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store;

public partial class R1999Store : ObservableObject
{
    public static R1999Store Instance = new();

    [ObservableProperty] public R1999State state = R1999State.Factory();

    public void Reduce(EventAction action)
    {
        State = R1999Reducer.Reduce(State, action);
    }
}