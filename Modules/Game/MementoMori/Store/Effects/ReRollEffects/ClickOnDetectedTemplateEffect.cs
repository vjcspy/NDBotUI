using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class ClickOnDetectedTemplateEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload ||
            baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint) return CoreAction.Empty;

        // Click
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null) return CoreAction.Empty;
        var isClicked = false;
        MoriTemplateKey[] clickOnMoriTemplateKeys =
        [
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.StartStartButton,
            MoriTemplateKey.IconChar1,
            MoriTemplateKey.ChallengeButton,
            MoriTemplateKey.TapToClose,
            MoriTemplateKey.BossBattleButton,
            MoriTemplateKey.SelectButton,
            MoriTemplateKey.ButtonClaim,
           
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            // case MoriTemplateKey.IconSpeakBeginningFirst:
            //     await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
            //     await Task.Delay(250);
            //     await emulatorConnection.ClickPPointAsync(new PPoint(49.6f, 81.4f));
            //     isClicked = true;
            //     break;
            
            case MoriTemplateKey.TextSelectFirstCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(13.7f,56.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectSecondCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(21.5f,56.7f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectThirdCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(13.9f,57.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectFourCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(22.3f,57.2f));
                isClicked = true;
                break;
            case MoriTemplateKey.PowerLevelUpText:
                await emulatorConnection.ClickPPointAsync(new PPoint(20.8f,94f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickLevelUpText:
                await emulatorConnection.ClickPPointAsync(new PPoint(75.2f,82.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickEquipAllText:
                await emulatorConnection.ClickPPointAsync(new PPoint(35.8f,82.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickQuestText:
                await emulatorConnection.ClickPPointAsync(new PPoint(44f,95f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickTheTownText:
                await emulatorConnection.ClickPPointAsync(new PPoint(50f,47.3f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideSelectTownButton:
                // select town and next
                await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                await Task.Delay(1500);
                await emulatorConnection.ClickPPointAsync(new PPoint(88.2f,89.6f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickRewardText:
                await emulatorConnection.ClickPPointAsync(new PPoint(51.6f,67.3f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickLevelUpImmediatelyText:
                await emulatorConnection.ClickPPointAsync(new PPoint(21f,94.7f));
                await Task.Delay(1500);
                // spam click all char
                await emulatorConnection.ClickPPointAsync(new PPoint(16.9f,31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(28.5f,31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(38.9f,31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(49.9f,31.8f));
                await Task.Delay(500);
                isClicked = true;
                break;
            
            case MoriTemplateKey.PartyInformation:
                // Khi không còn action gì mà hiện lên bảng Party Info thì nhấn begin battle
                await emulatorConnection.ClickPPointAsync(new PPoint(85f,88.2f));
                isClicked = true;
                break;
            default:
            {
                if (clickOnMoriTemplateKeys.Contains(detectedTemplatePoint.MoriTemplateKey))
                {
                    Logger.Info($"Click template {detectedTemplatePoint.MoriTemplateKey} on {detectedTemplatePoint.Point}");
                    await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }


        return isClicked ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload) : CoreAction.Empty;
    }
}