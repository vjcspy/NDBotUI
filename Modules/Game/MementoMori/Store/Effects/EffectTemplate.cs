using System.Reactive.Linq;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class EffectTemplate
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static EventAction Process(EventAction action)
    {
        Logger.Info("Processing Mori EffectTemplate");
        if (EmulatorManager.Instance.EmulatorConnections.Count != 1) return CorAction.Empty;

        var emulator = EmulatorManager.Instance.EmulatorConnections[0];
        var resolution = emulator.GetScreenResolution();

        Logger.Debug($"Screen resolution: {resolution}");

        return CorAction.Empty;
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream.OfAction([MoriAction.TriggerManually]).Select(Process);
    }
}