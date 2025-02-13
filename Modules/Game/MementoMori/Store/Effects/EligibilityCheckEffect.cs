using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class EligibilityCheckEffect : EffectBase
{
    // protected override bool GetForceEligible()
    // {
    //     return true;
    // }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.ToggleStartStopMoriReRoll];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            await Task.Delay(0);

            var isRunning = AppStore.Instance.MoriStore.State.IsReRollJobRunning(baseActionPayload.EmulatorId);

            if (isRunning)
            {
                return MoriAction.EligibilityCheck.Create(baseActionPayload);
            }
        }

        return CoreAction.Empty;
    }
}