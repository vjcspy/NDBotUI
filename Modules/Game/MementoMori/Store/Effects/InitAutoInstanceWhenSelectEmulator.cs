using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class InitAutoInstanceWhenSelectEmulator : EffectBase
{
    protected override bool GetForceEligible()
    {
        return true;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EmulatorAction.SelectEmulatorConnection];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info($"[InitAutoInstanceWhenSelectEmulator] Start {action}");
        if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;

        var emulator = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulator is null) return CoreAction.Empty;

        var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(emulator.Id);

        if (gameInstance is null)
        {
            /* init new game instance for emulator connection */
            AppStore.Instance.MoriStore.State.GameInstances.Add(
                new GameInstance(
                    EmulatorId: emulator.Id,
                    State: AutoState.Off,
                    Status: "",
                    JobType: MoriJobType.None,
                    JobReRollState: new JobReRollState()
                )
            );
        }


        return MoriAction.InitMoriSuccess.Create(new BaseActionPayload(baseActionPayload.EmulatorId));
    }
}