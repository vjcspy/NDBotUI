using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenCouldNotDetectScreenEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.CouldNotDetectScreen,];
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return currentStatus < R1999ReRollStatus.SaveResultOk;
            }
        }

        return false;
    }

    protected override bool IsParallel()
    {
        return false;
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload)
        {
            return CoreAction.Empty;
        }

        Logger.Info("Spam click when could not detect template");
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection == null)
        {
            return CoreAction.Empty;
        }

        await emulatorConnection.ClickPPointAsync(new PPoint(89f, 6f));
        await Task.Delay(250);
        // await emulatorConnection.ClickPPointAsync(new PPoint(95.6f, 6.8f));


        return CoreAction.Empty;
    }
}