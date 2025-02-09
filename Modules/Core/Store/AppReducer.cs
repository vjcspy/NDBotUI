namespace NDBotUI.Modules.Core.Store;

public class AppReducer
{
    public static AppState Reduce(AppState model, object action)
    {
        return action switch
        {
            AppAction.Increment => model with { Count = model.Count + 1 },
            AppAction.Decrement => model with { Count = model.Count - 1 },
            AppAction.Reset => AppState.factory(),
            _ => model
        };
    }
}