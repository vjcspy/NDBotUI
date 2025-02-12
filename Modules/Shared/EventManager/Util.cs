using System;
using System.Linq;
using System.Reactive.Linq;

namespace NDBotUI.Modules.Shared.EventManager;

public static class ObservableExtensions
{
    public static IObservable<EventAction> OfAction(
        this IObservable<EventAction> source,
        params IEventActionFactory<object?>[] actions)
    {
        return source.Where(action => actions
            .Select(f => f.Type)
            .Contains(action.Type));
    }
}