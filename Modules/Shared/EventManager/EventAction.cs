using System;

namespace NDBotUI.Modules.Shared.EventManager;

public class EventAction(object type, object? payload = null)
{
    private Guid? _correlationId;
    public object Type { get; } = type;
    public object? Payload { get; } = payload;

    public Guid? CorrelationId
    {
        get => _correlationId;
        set
        {
            if (value == null) throw new ArgumentException("Correlation ID cannot be null");
            if (_correlationId != null)
                throw new InvalidOperationException("Attempted to set correlationId when it already exists");

            _correlationId = value;
        }
    }

    public override string ToString()
    {
        return $"Type: {Type} - Payload: {Payload?.GetType()}";
    }
}

public class CoreAction
{
    public static readonly EventAction Empty = new("EMPTY_ACTION");
}