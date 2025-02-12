using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class CheckCurrentIfSkipMovie
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Processing Mori EffectTemplate");

        if (action.Payload is not BaseActionPayload baseActionPayload) return CorAction.Empty;
        var emulator = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId)!;

        var resolution = emulator.GetScreenResolution();

        Logger.Debug($"Screen resolution: {resolution}");

        return CorAction.Empty;
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream.OfAction([MoriAction.TriggerManually]).FilterBaseAction().SelectMany(Process);
    }
}