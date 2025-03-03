using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class WhenGetPresentsAndRoll : ScanTemplateEffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.GetPresents,
        ReRollStatus.GotPresents,
        ReRollStatus.RollX1,
    ];

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen,];
    }

    protected override bool IsParallel()
    {
        return false;
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance != null)
            {
                var currentStatus = gameInstance.JobReRollState.ReRollStatus;

                return VALID_STATUS.Contains(currentStatus);
            }
        }

        return false;
    }


    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Process on eligibility level check");
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint)
        {
            return CoreAction.Empty;
        }

        // Click
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null)
        {
            return CoreAction.Empty;
        }

        var gameInstance =
            AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        var isClicked = false;
        MoriTemplateKey[] clickOnMoriTemplateKeys =
        [
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.SkipSceneShotButton,
            MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
            MoriTemplateKey.StartStartButton, // truog hop bi disss
            MoriTemplateKey.InvokeCloseButton,
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            case MoriTemplateKey.ErrorHeaderPopup:
            {
                Logger.Info("Click Close Error");
                await emulatorConnection.ClickPPointAsync(new PPoint(50.4f, 61.5f));
                isClicked = true;
                break;
            }

            case MoriTemplateKey.HomePresentsIcon:
            {
                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GetPresents)
                {
                    await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                    isClicked = true;
                    break;
                }

                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GotPresents)
                {
                    // click invoke
                    await emulatorConnection.ClickPPointAsync(new PPoint(67.5f, 94.5f));
                    isClicked = true;
                    break;
                }

                break;
            }

        case MoriTemplateKey.BossBattleButton:
            {
                // click home
                await emulatorConnection.ClickPPointAsync(new PPoint(9.2f, 94.8f));
                isClicked = true;
                break;
            }
            case MoriTemplateKey.PresentsBoxHeader:
            {
                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GetPresents)
                {
                    // click claim
                    await emulatorConnection.ClickPPointAsync(new PPoint(84.9f, 80.1f));
                    await Task.Delay(500);
                    await emulatorConnection.ClickPPointAsync(new PPoint(50.1f, 88.4f));
                    return MoriAction.GotPresents.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GotPresents)
                {
                    // click close
                    await emulatorConnection.ClickPPointAsync(new PPoint(51.3f, 89.9f));
                    isClicked = true;
                }

                break;
            }

            case MoriTemplateKey.NotHaveEnoughDiamondText:
            {
                return MoriAction.RollX1.Create(baseActionPayload);
            }

            case MoriTemplateKey.BannerNewbie:
            {
                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GetPresents)
                {
                    // click home
                    await emulatorConnection.ClickPPointAsync(new PPoint(9.2f, 94.8f));
                    return MoriAction.GotPresents.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GotPresents)
                {
                    // click banner
                    await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                    await Task.Delay(500);
                    // click invoke 10
                    await emulatorConnection.ClickPPointAsync(new PPoint(90.5f, 80.8f));
                    await Task.Delay(1000);
                    isClicked = true;
                }

                break;
            }

            case MoriTemplateKey.LoginClaimButton:
            case MoriTemplateKey.ButtonClaim:
                await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                await Task.Delay(1000);
                // click outside
                await emulatorConnection.ClickPPointAsync(new PPoint(98.4f, 46.3f));
                break;
            case MoriTemplateKey.HomeIconBpText:
            case MoriTemplateKey.HomeNewPlayerText:
            {
                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GetPresents)
                {
                    // click claim
                    await emulatorConnection.ClickPPointAsync(new PPoint(84.9f, 80.1f));
                    await Task.Delay(500);
                    await emulatorConnection.ClickPPointAsync(new PPoint(50.1f, 88.4f));
                    return MoriAction.GotPresents.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == ReRollStatus.GotPresents)
                {
                    // click invoke
                    await emulatorConnection.ClickPPointAsync(new PPoint(67.5f, 93.8f));
                    isClicked = true;
                }
                break;
            }

            default:
            {
                if (clickOnMoriTemplateKeys.Contains(detectedTemplatePoint.MoriTemplateKey))
                {
                    Logger.Info(
                        $"Click template {detectedTemplatePoint.MoriTemplateKey} on {detectedTemplatePoint.Point}"
                    );
                    await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }

        return isClicked ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload) : CoreAction.Empty;
    }
}