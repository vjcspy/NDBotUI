using System;
using System.Reactive.Linq;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.AutoCore.Extensions;

public static class RxFilterBaseActionExtension
{
    public static IObservable<T> FilterBaseAction<T>(this IObservable<T> source)
    {
        return source.Where(action =>
        {
            if (action is not EventAction<object?> eventAction) return false;

            if (eventAction.Payload is not BaseActionPayload baseActionPayload) return false;

            var emulator = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            return emulator is not null;
        });
    }
}