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

public class ReRollHomeStep3Effect : EffectBase
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

                return currentStatus == EteReRollStatus.Step3HomeScreen;
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

        switch (detectTemplatePoint.TemplateKey)
        {

            case EteTemplateKey.SkipButton:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(92.3f, 8.8f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.MissionStartBtn:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(13.5f, 28.9f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.Mission1Task:
            case EteTemplateKey.Mission1Header:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(6.8f, 5.0f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.HomeSupplyBtn:
            {
                return EteAction.GetReward.Create(baseActionPayload);
            }

            case EteTemplateKey.ReplenishDailyBtn:
            {
                await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(92f, 13.6f));
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