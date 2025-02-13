namespace NDBotUI.Modules.Shared.EventManager;

public interface IEventActionFactory
{
    object Type { get; }
    EventAction Create(object? payload);
}

public class EventActionFactory(object type) : IEventActionFactory
{
    public object Type { get; } = type;

    public EventAction Create(object? payload = null)
    {
        return new EventAction(Type, payload);
    }
}