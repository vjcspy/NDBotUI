using System;
using System.Linq;
using System.Reactive.Linq;

namespace NDBotUI.Modules.Shared.EventManager;

public static class ObservableExtensions
{
    public static IObservable<EventAction<object?>> OfAction(
        this IObservable<EventAction<object?>> source,
        params IEventActionFactory<object?>[] actions)
    {
        return source.Where(action => actions
            .Select(f => f.Type)
            .Contains(action.Type));
    }
}