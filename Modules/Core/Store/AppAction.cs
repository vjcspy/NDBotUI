using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public class AppAction
{
    public enum Type
    {
        Increment,
        Decrement,
        Reset
    }

    public static EventActionFactory<object?> Increment = new(Type.Increment);
    public static EventActionFactory<object?> Decrement = new(Type.Decrement);
    public static EventActionFactory<object?> Reset = new(Type.Reset);
}