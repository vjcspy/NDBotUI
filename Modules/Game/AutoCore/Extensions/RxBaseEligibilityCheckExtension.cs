using System;
using System.Reactive.Linq;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.AutoCore.Extensions;

public static class RxBaseEligibilityCheckExtension
{
    public static IObservable<T> FilterBaseEligibility<T>(this IObservable<T> source, bool forceEligible = false)
    {
        return source.Where(action =>
        {
            if (forceEligible)
            {
                return true;
            }

            if (action is not EventAction eventAction) return false;

            if (eventAction.Payload is not BaseActionPayload baseActionPayload) return true;

            /* Chỉ chạy khi state của auto là On */
            var currentGameInstance =
                AppStore.Instance.MoriStore.State.GameInstances.Find(instance =>
                    instance.EmulatorId == baseActionPayload.EmulatorId);
            if (currentGameInstance is null || currentGameInstance.State != AutoState.On) return false;

            /* __ */

            return true;
        });
    }
}