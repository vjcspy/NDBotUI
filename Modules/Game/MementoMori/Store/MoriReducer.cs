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
                                    JobReRollState: new JobReRollState()
                                )
                            )
                        };
                    }


                    state = state with
                    {
                        GameInstances = state.GameInstances.Map(gi =>
                            gi.EmulatorId == baseActionPayload.EmulatorId
                                ? gi with
                                {
                                    JobReRollState = gi.JobReRollState with
                                    {
                                        ReRollStatus = ReRollStatus.Initialized
                                    }
                                }
                                : gi
                        )
                    };
                }

                return state;
            }

            default:
                return state;
        }
    }
}