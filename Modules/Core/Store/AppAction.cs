using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Core.Store;

public class AppAction
{
    public enum Type
    {
        Increment,
        Decrement,
        Reset,
    }

    public static EventActionFactory Increment = new(Type.Increment);
    public static EventActionFactory Decrement = new(Type.Decrement);
    public static EventActionFactory Reset = new(Type.Reset);
}