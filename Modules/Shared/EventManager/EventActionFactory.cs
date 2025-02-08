namespace NDBotUI.Modules.Shared.EventManager;

public interface IEventActionFactory<T>
{
    string Type { get; }
    EventAction<T?> Create(T? payload);
}

public class EventActionFactory<T>(string type) : IEventActionFactory<T>
{
    public string Type { get; } = type;

    public EventAction<T?> Create(T? payload = default)
    {
        return new EventAction<T?>(Type, payload);
    }
}