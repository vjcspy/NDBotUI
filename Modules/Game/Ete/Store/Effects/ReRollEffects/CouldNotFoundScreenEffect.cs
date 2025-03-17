using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store.Effects.ReRollEffects;

public class CouldNotFoundScreenEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EteAction.CouldNotDetectScreen,];
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.EteStore.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return currentStatus == EteReRollStatus.Start
                       || currentStatus == EteReRollStatus.Step1HomeScreen
                       || currentStatus == EteReRollStatus.Step3HomeScreen;
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

        var gameInstance =
            AppStore.Instance.EteStore.State.GetGameInstance(baseActionPayload.EmulatorId);



        await emulatorConnection.ClickPPointAsync(new PPoint(93, 6.2f));
        if (gameInstance?.JobReRollState.ReRollStatus == EteReRollStatus.Start)
        {
            await emulatorConnection.ClickPPointAsync(new PPoint(82.2f, 74.7f));
        }
        await Task.Delay(250);
        // await emulatorConnection.ClickPPointAsync(new PPoint(95.6f, 6.8f));


        return CoreAction.Empty;
    }
}