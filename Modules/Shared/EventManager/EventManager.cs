using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using NDBotUI.Modules.Core.Store;
using NLog;

namespace NDBotUI.Modules.Shared.EventManager;

public class RxEventManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly Subject<EventAction> ActionSubject = new();

    public static void Dispatch(EventAction action)
    {
        Logger.Info("Dispatching event " + action.Type);
        action.CorrelationId ??= Guid.NewGuid();

        AppStore.Instance.Reduce(action);

        ActionSubject.OnNext(action);
    }

    public static void RegisterEvent(string[] eventTypes, RxEventHandler eventHandler)
    {
        ActionSubject
            .Where(action => eventTypes.Length == 0 || eventTypes.Contains(action.Type))
            .SelectMany(
                originalEvent =>
                    eventHandler(Observable.Return(originalEvent))
                        .SubscribeOn(TaskPoolScheduler.Default)
                        .Select(handledEvent => new { Original = originalEvent, Handled = handledEvent, })
            )
            .ObserveOn(Scheduler.Default)
            .Where(events => events.Handled != CoreAction.Empty)
            .Subscribe(
                events =>
                {
                    var originEvent = events.Original;
                    var handledEvent = events.Handled;

                    if (originEvent.CorrelationId != null && handledEvent.CorrelationId == null)
                    {
                        handledEvent.CorrelationId = originEvent.CorrelationId;
                    }

                    Dispatch(handledEvent);
                },
                error => Logger.Error($"Error in event stream: {error.Message}")
            );
    }

    public static void RegisterEvent(object eventEffectInstance)
    {
        var methods = eventEffectInstance
            .GetType()
            .GetMethods()
            .Where(m => m.GetCustomAttribute<EffectAttribute>() != null && m.ReturnType == typeof(RxEventHandler))
            .ToList();

        foreach (var method in methods)
        {
            var effectAttribute = method.GetCustomAttribute<EffectAttribute>();
            var eventTypes = effectAttribute?.Types ?? [];

            var eventHandler = (RxEventHandler)method.Invoke(eventEffectInstance, null)!;

            RegisterEvent(eventTypes, eventHandler);
        }
    }

    public static void RegisterEvent(object[] eventEffectInstances)
    {
        foreach (var eventEffectInstance in eventEffectInstances)
        {
            RegisterEvent(eventEffectInstance);
        }
    }
}