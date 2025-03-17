using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.Ete.Helper;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store.Effects;

public class InitEteEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EteAction.InitEte,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        await Task.Delay(0);
        // Init template for scanning image
        EteScreenDetectorDataHelper
            .GetInstance()
            .LoadData();

        return CoreAction.Empty;
    }
}