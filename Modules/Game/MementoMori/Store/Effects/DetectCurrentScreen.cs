using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class DetectCurrentScreen : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return
        [
            MoriAction.EligibilityCheck
        ];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info($"Detect current screen");

        return CoreAction.Empty;
    }
}