using System;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateEffectEligibilityLevel : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.EligibilityLevelCheck,];
    }

    protected override Task<EventAction> Process(EventAction action)
    {
        throw new NotImplementedException();
    }
}