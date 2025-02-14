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
            MoriTemplateKey.ChallengeButton,
            MoriTemplateKey.TapToClose
           
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            case MoriTemplateKey.IconSpeakBeginningFirst:
                await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                await Task.Delay(250);
                await emulatorConnection.ClickPPointAsync(new PPoint(49.6f, 81.4f));
                isClicked = true;
                break;
            
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