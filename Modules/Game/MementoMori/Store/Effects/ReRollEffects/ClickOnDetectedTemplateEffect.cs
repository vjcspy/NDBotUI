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
            MoriTemplateKey.ChallengeButton
        ];

        if (detectedTemplatePoint.MoriTemplateKey is MoriTemplateKey.IconSpeakBeginningFirst)
        {
            await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
            await Task.Delay(250);
            await emulatorConnection.ClickPPointAsync(new PPoint(49.6f, 81.4f));
            isClicked = true;
        }
        else if (detectedTemplatePoint.MoriTemplateKey is MoriTemplateKey.TextSelectFirstCharToTeam)
        {
            await emulatorConnection.ClickPPointAsync(new PPoint(13.7f,56.4f));
            isClicked = true;
        }
        else if (clickOnMoriTemplateKeys.Contains(detectedTemplatePoint.MoriTemplateKey))
        {
            Logger.Info($"Click template {detectedTemplatePoint.MoriTemplateKey} on {detectedTemplatePoint.Point}");
            await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
            isClicked = true;
        }


        return isClicked ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload) : CoreAction.Empty;
    }
}