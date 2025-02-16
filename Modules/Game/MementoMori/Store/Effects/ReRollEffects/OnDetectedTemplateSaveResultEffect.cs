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
            // MoriTemplateKey.CharacterGrowthPossible,
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.SkipSceneShotButton,
            // MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
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
                    return MoriAction.ResetUserData.Create(baseActionPayload);
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
            return false;
        }

        var characterTabPoint = await ScanTemplateAsync(
            [MoriTemplateKey.CharacterTabHeader],
            emulatorConnection,
            screenshot
        );

        if (characterTabPoint.Length > 0)
        {
            await SkiaHelper.SaveScreenshot(
                emulatorConnection,
                ImageHelper.GetImagePath("character", "results/characters"),
                screenshot
            );

            return true;
        }
        else
        {
            return false;
        }
    }
}