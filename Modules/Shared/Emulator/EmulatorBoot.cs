using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NDBotUI.Modules.Core.Attributes;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.Emulator.Store.Effects;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator;

public class EmulatorBoot
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    [SingleCall]
    public static void Boot()
    {
        RxEventManager.RegisterEvent([new EmulatorEffect(), new RefreshEmulatorEffect()]);
        Observable.Interval(TimeSpan.FromSeconds(10))
            .ObserveOn(Scheduler.Default)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                _ =>
                {
                    RxEventManager.Dispatch(
                        EmulatorAction.EmulatorRefresh.Create());
                },
                ex => Console.WriteLine($"Error: {ex.Message}"),
                () => Console.WriteLine("Completed"));
    }
}