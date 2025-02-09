namespace NDBotUI.Modules.Shared.EventManager;

public interface IEventActionFactory<T>
{
    object Type { get; }
    EventAction<T?> Create(T? payload);
}

public class EventActionFactory<T>(object type) : IEventActionFactory<T>
{
    public object Type { get; } = type;

    public EventAction<T?> Create(T? payload = default)
    {
        return new EventAction<T?>(Type, payload);
    }
}