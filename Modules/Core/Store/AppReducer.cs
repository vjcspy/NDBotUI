using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public class AppReducer
{
    public static AppState Reduce(AppState model, EventAction<object?> action)
    {
        return action.Type switch
        {
            AppAction.Type.Increment => model with { Count = model.Count + 1 },
            AppAction.Type.Decrement => model with { Count = model.Count - 1 },
            AppAction.Type.Reset => AppState.factory(),
            _ => model
        };
    }
}