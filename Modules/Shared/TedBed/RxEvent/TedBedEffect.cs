using System;
using System.Reactive.Linq;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.TedBed.RxEvent;

public class TedBedEffect
{
    private static EventAction<object?> Process(EventAction<object?> action)
    {
        Console.WriteLine("Processing event " + action.Type);
        return TestBedAction.BAR_ACTION.Create();
    }


    [Effect]
    public RxEventHandler HandleUserEvents()
    {
        return upstream => upstream.OfAction([TestBedAction.FOO_ACTION]).Select(Process);
    }
}