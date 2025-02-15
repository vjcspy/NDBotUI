using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LanguageExt;
using NDBotUI.Modules.Core.Attributes;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori;

public class MoriBoot
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    [SingleCall]
    public static void Boot()
    {
        Logger.Info("Boot Mori...");
        RxEventManager.RegisterEvent(MoriEffect.Effects);

        Observable.Interval(TimeSpan.FromSeconds(2))
            .ObserveOn(Scheduler.Default)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                _ =>
                {
                    AppStore.Instance.MoriStore.State.GameInstances.Iter(x =>
                    {
                        if (x.State == AutoState.On)
                            RxEventManager.Dispatch(
                                MoriAction.TriggerScanCurrentScreen.Create(new BaseActionPayload(x.EmulatorId)));
                    });
                },
                ex => Console.WriteLine($"Error: {ex.Message}"),
                () => Console.WriteLine("Completed"));
    }
}