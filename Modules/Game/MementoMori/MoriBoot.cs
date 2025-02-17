using System;
using System.Reactive.Linq;
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

        Observable
            .Interval(TimeSpan.FromSeconds(2))
            .SelectMany(
                _ => AppStore.Instance.MoriStore.State.GameInstances.ToObservable()
            ) // Chuyển danh sách thành Observable
            .Where(x => x.State == AutoState.On) // Lọc các instance cần xử lý
            .SelectMany(
                x =>
                    Observable
                        .Return(x) // Đảm bảo xử lý tuần tự từng phần tử
                        .Delay(TimeSpan.FromSeconds(2)) // Chờ trước khi gửi tiếp sự kiện tiếp theo
            )
            .Subscribe(
                x =>
                {
                    RxEventManager.Dispatch(
                        MoriAction.TriggerScanCurrentScreen.Create(new BaseActionPayload(x.EmulatorId))
                    );
                },
                ex => Logger.Error(ex, $"Error: {ex.Message}")
            );
    }
}