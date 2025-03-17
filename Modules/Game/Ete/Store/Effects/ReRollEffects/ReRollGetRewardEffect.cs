using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store.Effects.ReRollEffects;

public class ReRollGetRewardEffect : EffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        EteTemplateKey.ConfirmButton,
        EteTemplateKey.ConfirmButton1,
        EteTemplateKey.CloseDailyRwBttn,
    ];

    protected override bool IsParallel()
    {
        return false;
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

                return currentStatus == EteReRollStatus.GetReward || currentStatus == EteReRollStatus.GotEmailReward;
            }
        }

        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EteAction.DetectScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process ReRollHomeStep3Effect");
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectTemplatePoint detectTemplatePoint)
        {
            return CoreAction.Empty;
        }

        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null)
        {
            return CoreAction.Empty;
        }

        var isClicked = false;

        var gameInstance =
            AppStore.Instance.EteStore.State.GetGameInstance(baseActionPayload.EmulatorId);

        if (gameInstance is null)
        {
            return CoreAction.Empty;
        }

        switch (detectTemplatePoint.TemplateKey)
        {

            case EteTemplateKey.SkipButton:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(92.3f, 8.8f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.HomeSupplyBtn:
            {
                if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.GetReward)
                {
                    await emulatorConnection.ClickPPointAsync(new PPoint(5.9f, 20.9f));
                    isClicked = true;
                }else if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.GotEmailReward)
                {
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    return EteAction.DoReRoll.Create(baseActionPayload);
                }
                break;
            }

            case EteTemplateKey.EmailClaimAllBtn:
            {
                if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.GetReward)
                {
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(25.9f, 85.9f));
                    return EteAction.GotEmailReward.Create(baseActionPayload);
                }
                else
                {
                    // click back
                    await emulatorConnection.ClickPPointAsync(new PPoint(4.7f, 5.7f));
                    isClicked = true;
                }

                break;
            }

            default:
            {
                if (_clickOnTemplateKeys.Contains(detectTemplatePoint.TemplateKey))
                {
                    Logger.Info(
                        $"Click template {detectTemplatePoint.TemplateKey} on {detectTemplatePoint.Point}"
                    );
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }

        return isClicked ? R1999Action.ClickedAfterDetectedScreen.Create(baseActionPayload) : CoreAction.Empty;
    }
}