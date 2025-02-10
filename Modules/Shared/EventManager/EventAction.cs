﻿using System;

namespace NDBotUI.Modules.Shared.EventManager;

public class EventAction<T>(object type, T? payload = default)
{
    public object Type { get; } = type;
    public T? Payload { get; } = payload;

    private Guid? _correlationId;

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

    public T AssertPayload()
    {
        if (Payload is not T)
        {
            throw new InvalidCastException(
                $"Payload is not of type {typeof(T).Name}, but is of type {Payload?.GetType().Name ?? "null"}");
        }

        return (T)Payload;
    }
}

public class CorAction
{
    public static readonly EventAction<object?> Empty = new("EMPTY_ACTION");
}