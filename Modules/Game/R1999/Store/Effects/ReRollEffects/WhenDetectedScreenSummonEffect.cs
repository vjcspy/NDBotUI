using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenDetectedScreenSummonEffect : EffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
    ];

    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.DetectScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process WhenDetectedScreenSummonEffect");
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


        switch (detectTemplatePoint.TemplateKey)
        {
            case R1999TemplateKey.Chapter5Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.5f, 7.1f));
                isClicked = true;
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

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return currentStatus == R1999ReRollStatus.FinishQuest;
            }
        }

        return false;
    }
}