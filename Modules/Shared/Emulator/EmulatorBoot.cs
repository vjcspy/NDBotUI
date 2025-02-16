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
        RxEventManager.RegisterEvent([new EmulatorEffect(), new RefreshEmulatorEffect(),]);

        var firstRun = Observable
            .Timer(TimeSpan.FromSeconds(15)) // Chạy lần đầu sau 5s
            .Do(_ => RxEventManager.Dispatch(EmulatorAction.EmulatorRefresh.Create()));
        var periodicRun = Observable
            .Interval(TimeSpan.FromSeconds(30)) // Chạy mỗi 30s
            .Do(_ => RxEventManager.Dispatch(EmulatorAction.EmulatorRefresh.Create()));
        firstRun
            .Concat(periodicRun) // Ghép hai luồng lại, đảm bảo chạy tuần tự
            .ObserveOn(Scheduler.Default)
            .SubscribeOn(Scheduler.Default)
            .Subscribe(
                _ => { }, // Không cần xử lý, vì đã có `.Do` phía trên
                ex => Console.WriteLine($"Error: {ex.Message}"),
                () => Console.WriteLine("Completed")
            );
    }
}