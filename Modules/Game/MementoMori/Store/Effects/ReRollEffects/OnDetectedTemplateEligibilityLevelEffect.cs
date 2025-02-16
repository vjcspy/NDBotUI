using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateEligibilityLevelEffect : EffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.EligibilityLevelCheck,
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
            MoriTemplateKey.CharacterGrowthPossible,
        ];
        switch (detectedTemplatePoint.MoriTemplateKey)
        {
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