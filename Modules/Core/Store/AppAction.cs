using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public enum AppAction
{
    Increment,
    Decrement,
    Reset
}

public class AppActionFactory
{
    public static EventActionFactory<object?> Increment = new(AppAction.Increment);
    public static EventActionFactory<object?> Decrement = new(AppAction.Decrement);
    public static EventActionFactory<object?> Reset = new(AppAction.Reset);
}