using LanguageExt;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriReducer
{
    public static MoriState Reduce(MoriState state, EventAction action)
    {
        switch (action.Type)
        {
            case MoriAction.Type.InitMoriSuccess:
            {
                if (action.Payload is BaseActionPayload baseActionPayload)
                {
                    var gameInstance = state.GetGameInstance(baseActionPayload.EmulatorId);
                    if (gameInstance == null)
                    {
                        state = state with
                        {
                            GameInstances = state.GameInstances.Add(
                                new GameInstance(
                                    EmulatorId: baseActionPayload.EmulatorId,
                                    State: AutoState.Off,
                                    Status: "",
                                    JobType: MoriJobType.None,
                                    JobReRollState: new JobReRollState(ReRollStatus: ReRollStatus.Open)
                                )
                            )
                        };
                    }
                }

                return state;
            }

            /* _____________________________ ReRoll _____________________________*/
            case MoriAction.Type.ToggleStartStopMoriReRoll:
            {
                var emulator = AppStore.Instance.EmulatorStore.State.SelectedEmulatorId;
                if (emulator is not { } emulatorId) return state;

                var isRunning = state.GameInstances
                    .Find(instance => instance.EmulatorId == emulatorId)
                    .Map(gameInstance => gameInstance.State == AutoState.On)
                    .Match(Some: x => x, None: () => false);

                state = state with
                {
                    GameInstances = state.GameInstances.Map(gameInstance =>
                        gameInstance.EmulatorId == emulatorId
                            ? gameInstance with
                            {
                                State = isRunning ? AutoState.Off : AutoState.On,
                                JobType = MoriJobType.ReRoll,
                                JobReRollState = gameInstance.JobReRollState with
                                {
                                    ReRollStatus = isRunning ? ReRollStatus.Open : ReRollStatus.Start,
                                }
                            }
                            : gameInstance
                    )
                };

                return state;
            }

            default:
                return state;
        }
    }
}