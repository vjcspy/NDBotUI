using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateLinkAccountEffect : ScanTemplateEffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.LinkAccount,
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
        Logger.Info("Process on link account");
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

        var isClicked = false;
        MoriTemplateKey[] clickOnMoriTemplateKeys =
        [
            // MoriTemplateKey.CharacterGrowthPossible,
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.SkipSceneShotButton,
            // MoriTemplateKey.ReturnToTitleButton,
            // MoriTemplateKey.StartSettingButton,
            // MoriTemplateKey.ResetGameDataButton,
            MoriTemplateKey.DownloadUpdateButton,
            MoriTemplateKey.StartStartButton,
            MoriTemplateKey.GuideClickDownButton,

            // MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
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
                // click vào menu
                await emulatorConnection.ClickPPointAsync(new PPoint(97f, 3.6f));
                isClicked = true;
                break;
            }

            case MoriTemplateKey.ReturnToTitleButton:
            {
                // click vào account link
                await emulatorConnection.ClickPPointAsync(new PPoint(80.5f, 46.8f));
                isClicked = true;
                break;
            }

            case MoriTemplateKey.EnterYourLinkAccountText:
            {
                // click vào link button
                await emulatorConnection.ClickPPointAsync(new PPoint(65.8f, 28.2f));
                isClicked = true;
                break;
            }
            case MoriTemplateKey.PerformAccountLink:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(58.7f, 83.1f));
                isClicked = true;
                break;
            }
            case MoriTemplateKey.SetLinkPassword:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(64.9f, 59.1f));
                isClicked = true;
                break;
            }

            case MoriTemplateKey.EnterLinkInfo:
            {
                // spam 1 click truoc
                await emulatorConnection.ClickPPointAsync(new PPoint(51.1f, 53.4f));
                await Task.Delay(500);
                // click nhap pass
                await emulatorConnection.ClickPPointAsync(new PPoint(48.8f, 62.1f));
                await Task.Delay(500);

                emulatorConnection.ClearInput();
                await Task.Delay(1000);
                emulatorConnection.SendText("123456aA");
                // spam 1 click ra ngoai
                await emulatorConnection.ClickPPointAsync(new PPoint(51.1f, 53.4f));

                await Task.Delay(1500);

                var screenshot = await emulatorConnection.TakeScreenshotAsync();

                if (screenshot is null)
                {
                    Logger.Error("Failed to take screenshot");
                    return CoreAction.Empty;
                }

                var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(emulatorConnection.Id);

                if (gameInstance?.JobReRollState.ResultId == null)
                {
                    Logger.Error("Could not get game instance");
                    return CoreAction.Empty;
                }

                var isFillPassSuccess = await ScanTemplateAsync(
                    [MoriTemplateKey.SavePassSuccess,],
                    emulatorConnection,
                    screenshot
                );
                if (isFillPassSuccess.Length > 0)
                {
                    await SkiaHelper.SaveScreenshot(
                        emulatorConnection,
                        ImageHelper.GetImagePath(gameInstance.JobReRollState.ResultId.ToString()!, "results/accounts"),
                        screenshot
                    );
                    // click finished
                    await emulatorConnection.ClickPPointAsync(new PPoint(58.3f, 81.9f));
                    return MoriAction.ResetUserData.Create(baseActionPayload);
                }

                Logger.Error("Failed to fill pass");
                break;
            }

            case MoriTemplateKey.CharacterTabHeader:
            case MoriTemplateKey.PartyInformation:
            case MoriTemplateKey.BossBattleButton:
            {
                // click vào home
                await emulatorConnection.ClickPPointAsync(new PPoint(8.7f, 95.2f));
                isClicked = true;
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

        return isClicked
            ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload)
            : CoreAction.Empty;
    }


    private async Task<EventAction> WhenDone(BaseActionPayload baseActionPayload)
    {
        RxEventManager.Dispatch(
            MoriAction.ToggleStartStopMoriReRoll.Create(
                new BaseActionPayload(baseActionPayload.EmulatorId)
            )
        );
        await Task.Delay(3000);
        RxEventManager.Dispatch(
            MoriAction.ToggleStartStopMoriReRoll.Create(
                new BaseActionPayload(baseActionPayload.EmulatorId)
            )
        );

        return CoreAction.Empty;
    }
}