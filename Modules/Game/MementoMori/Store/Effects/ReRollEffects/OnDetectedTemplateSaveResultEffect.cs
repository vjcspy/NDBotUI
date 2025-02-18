using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateSaveResultEffect : ScanTemplateEffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.SaveResult,
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
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return VALID_STATUS.Contains(currentStatus);
            }
        }

        return false;
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Process on save result");
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
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.SkipSceneShotButton,
            MoriTemplateKey.CharacterGrowthPossible,
            MoriTemplateKey.StartStartButton,
            MoriTemplateKey.GuideClickDownButton,
            // MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            case MoriTemplateKey.BossBattleButton:
            {
                Logger.Info("Click Character");
                await emulatorConnection.ClickPPointAsync(new PPoint(21.3f, 94.6f));
                isClicked = true;
                break;
            }
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
                isClicked = true;
                break;
            case MoriTemplateKey.BeforeChallengeEnemyPower22:
            case MoriTemplateKey.BeforeChallengeEnemyPower23:
            {
                var screenshot = await emulatorConnection.TakeScreenshotAsync();

                if (screenshot is null)
                {
                    Logger.Error("Failed to take screenshot");
                    break;
                }

                var characterTabPoint = await ScanTemplateAsync(
                    [MoriTemplateKey.PartyInformation,],
                    emulatorConnection,
                    screenshot
                );

                if (characterTabPoint.Length > 0)
                {
                    // close party information
                    await emulatorConnection.ClickPPointAsync(new PPoint(93.7f, 7.6f));
                    await Task.Delay(1000);
                    isClicked = true;
                }

                // spam click 1 lần nữa cho chắc ăn
                await emulatorConnection.ClickPPointAsync(new PPoint(99.5f, 52.6f));

                break;
            }

            case MoriTemplateKey.ChallengeButton:
                // click ra ngoai
                await emulatorConnection.ClickPPointAsync(new PPoint(0.8f, 34.9f));
                isClicked = true;
                break;

            case MoriTemplateKey.HomeIconBpText:
            case MoriTemplateKey.HomeNewPlayerText:
            {
                // click vào lại character tab
                await emulatorConnection.ClickPPointAsync(new PPoint(21.1f, 93.6f));
                isClicked = true;
                break;
            }

            case MoriTemplateKey.CharacterTabHeader:
            case MoriTemplateKey.PartyInformation:
            {
                var isSaved = await SaveResult(emulatorConnection);

                if (isSaved)
                {
                    return MoriAction.LinkAccount.Create(baseActionPayload);
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

        return isClicked
            ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload)
            : CoreAction.Empty;
    }


    protected async Task<bool> SaveResult(EmulatorConnection emulatorConnection)
    {
        var screenshot = await emulatorConnection.TakeScreenshotAsync();

        if (screenshot is null)
        {
            Logger.Error("Failed to take screenshot");
            return false;
        }

        var characterTabPoint = await ScanTemplateAsync(
            [MoriTemplateKey.CharacterTabHeader,],
            emulatorConnection,
            screenshot
        );
        var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(emulatorConnection.Id);

        if (gameInstance?.JobReRollState.ResultId == null)
        {
            Logger.Error("Could not get game instance");
            return false;
        }

        if (characterTabPoint.Length > 0)
        {
            await SkiaHelper.SaveScreenshot(
                emulatorConnection,
                ImageHelper.GetImagePath(gameInstance.JobReRollState.ResultId.ToString()!, "results/characters"),
                screenshot
            );

            return true;
        }

        Logger.Error("Could not find character tab header");
        return false;
    }
}