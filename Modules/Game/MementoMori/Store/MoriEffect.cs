using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.MementoMori.Store.Effects;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriEffect
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task<EventAction<object?>> Process(EventAction<object?> action)
    {
        Logger.Info("Mori effect started");

        // if (EmulatorManager.Instance.EmulatorConnections.Count != 1) return CorAction.Empty;
        //
        // var emulator = EmulatorManager.Instance.EmulatorConnections[0];
        //
        // if (await emulator.GetPointByImageAsync() is { } point)
        // {
        //     await emulator.clickOnPointAsync(point);
        // }

        return CorAction.Empty;
    }

    [Effect]
    public RxEventHandler HandlerScanCurrentScreen()
    {
        return upstream => upstream.OfAction([MoriAction.TriggerScanCurrentScreen]).SelectMany(Process);
    }

    public static readonly List<object> Effects = [new InitTemplateDataEffect()];
}