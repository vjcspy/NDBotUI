using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class WhenFoundCharacterGrowthPossible : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen,];
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            if (baseActionPayload.Data is DetectedTemplatePoint detectedTemplatePoint)
            {
                if (detectedTemplatePoint.MoriTemplateKey == MoriTemplateKey.CharacterGrowthPossible)
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        await Task.Delay(100);
        return MoriAction.EligibilityLevelCheck.Create(action.Payload);
    }
}