using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriReducer
{
    public static MoriState Reduce(MoriState model, EventAction action)
    {
        return action.Type switch
        {
            _ => model
        };
    }
}