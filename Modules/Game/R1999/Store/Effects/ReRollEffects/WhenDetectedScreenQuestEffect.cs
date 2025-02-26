using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenDetectedScreenQuestEffect: EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.DetectScreen];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectTemplatePoint detectTemplatePoint)
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

        Enum[] clickOnTemplateKeys =
        [
            R1999TemplateKey.SkipMovieBtn1,
        ];

        switch (detectTemplatePoint.TemplateKey)
        {
            default:
            {
                if (clickOnTemplateKeys.Contains(detectTemplatePoint.TemplateKey))
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