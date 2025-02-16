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

public class OnDetectedTemplateResetUserDataEffect : ScanTemplateEffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.ResetUserData,
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
        Logger.Info("Process on reset user data");
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
            MoriTemplateKey.ReturnToTitleButton,
            MoriTemplateKey.StartSettingButton,
            MoriTemplateKey.ResetGameDataButton,
            MoriTemplateKey.DownloadUpdateButton,
            
            // MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
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
            
            case MoriTemplateKey.ReturnToTitleHeader:
            {
                // click vào ok
                await emulatorConnection.ClickPPointAsync(new PPoint(58.8f, 61.7f));
                isClicked = true;
                break;
            }
            
            case MoriTemplateKey.ResetGameDataHeader:
            {
                // click vào reset
                await emulatorConnection.ClickPPointAsync(new PPoint(59.4f, 68.8f));
                isClicked = true;
                break;
            }   
            case MoriTemplateKey.StartStartButton:
            {
                // click vào reset
                await emulatorConnection.ClickPPointAsync(new PPoint(96.6f, 5.2f));
                isClicked = true;
                break;
            }
            
            case MoriTemplateKey.ConfirmGameDataResetHeader:
            {
                // click vào OK
                await emulatorConnection.ClickPPointAsync(new PPoint(59.0f, 62.2f));
                return await WhenDone(baseActionPayload);
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