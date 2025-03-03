using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class WhenFoundCharacterGrowthPossible : EffectBase
{
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
            if (baseActionPayload.Data is DetectedTemplatePoint detectedTemplatePoint)
            {
                if (detectedTemplatePoint.MoriTemplateKey == MoriTemplateKey.CharacterGrowthPossible)
                {
                    var gameInstance =
                        AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);
                    if (gameInstance != null && gameInstance.JobReRollState.ReRollStatus < ReRollStatus.NextChapter)
                    {
                        var currentLevelValid = gameInstance.JobReRollState.CurrentLevel != 0
                                                && gameInstance.JobReRollState.CurrentLevel >= 15
                                                && gameInstance.JobReRollState.CurrentLevel <= 112;
                        Logger.Info(
                            $"=>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Found character growth possible, current level: {gameInstance.JobReRollState.CurrentLevel}"
                        );
                        return currentLevelValid;
                    }

                    Logger.Info("=>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Not valid to do character growth");
                }
            }
        }

        return false;
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        await Task.Delay(100);
        return MoriAction.EligibilityLevelCheck.Create(action.Payload);
    }
}