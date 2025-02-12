namespace NDBotUI.Modules.Shared.EventManager;

public interface IEventActionFactory<T>
{
    object Type { get; }
    EventAction Create(T? payload);
}

public class EventActionFactory<T>(object type) : IEventActionFactory<T>
{
    public object Type { get; } = type;

    public EventAction Create(T? payload = default)
    {
        return new EventAction(Type, payload);
    }
}