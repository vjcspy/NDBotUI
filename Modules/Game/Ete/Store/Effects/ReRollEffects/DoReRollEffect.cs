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

public class DoReRollEffect : EffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        EteTemplateKey.ConfirmButton,
        EteTemplateKey.ConfirmButton1,
        EteTemplateKey.CloseDailyRwBttn,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.SupplySkipText,
        EteTemplateKey.SupplyExtraText,
        EteTemplateKey.SupplyConfirmBtn,
        EteTemplateKey.SupplyTapEmpty,
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

                return currentStatus >= EteReRollStatus.DoReRoll && currentStatus < EteReRollStatus.DoReRollCharOk;
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
        Logger.Info(">>Process DoReRollEffect");
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

            case EteTemplateKey.ReplenishDailyBtn:
            {
                await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(92f, 13.6f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.BannerCharDragon:
            {
                if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.DoReRoll)
                {
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    await Task.Delay(1000);
                    // click x10
                    await emulatorConnection.ClickPPointAsync(new PPoint(88.6f, 91.2f));
                    isClicked = true;
                }

                if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.DoReRollLackMoneyX10)
                {
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    await Task.Delay(1000);
                    // click x1
                    await emulatorConnection.ClickPPointAsync(new PPoint(72.1f, 91.2f));
                    isClicked = true;
                }

                break;
            }

            // bang confirm
            case EteTemplateKey.Supply10xTextConfirm:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(60f, 81f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.SupplyConsum1000Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(59f, 69.3f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.LackMoneyText:
            {
                if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.DoReRollLackMoneyX10)
                {
                    await emulatorConnection.ClickPPointAsync(new PPoint(36.8f, 69.5f));
                    return EteAction.ToggleStartStopReRoll.Create(baseActionPayload);
                }

                // cancel
                await emulatorConnection.ClickPPointAsync(new PPoint(36.8f, 69.5f));
                return EteAction.DoReRollLackMoneyX10.Create(baseActionPayload);
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