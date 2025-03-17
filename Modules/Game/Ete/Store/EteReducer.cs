using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.Ete.Typing;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Game.R1999.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteReducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static EteState Reduce(EteState state, EventAction action)
    {
        switch (action.Type)
        {
            case EmulatorAction.Type.EmulatorConnectSuccess:
            {
                if (action.Payload is Lst<EmulatorConnection> emulatorConnections)
                {
                    foreach (var emulatorConnection in emulatorConnections)
                    {
                        var gameInstance = state.GetGameInstance(emulatorConnection.Id);
                        if (gameInstance == null)
                        {
                            state = state with
                            {
                                GameInstances = state.GameInstances.Add(
                                    EteGameInstance.Factory(emulatorConnection.Id)
                                ),
                            };
                        }
                    }
                }

                return state;
            }

            case EteAction.Type.ToggleStartStopReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = EteJobReRollState.Factory();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = isRunning ? AutoState.Off : AutoState.On,
                                    JobType = EteJobType.ReRoll,
                                    JobReRollState = newJobReRollState with
                                    {
                                        ReRollStatus = isRunning ? EteReRollStatus.Open : EteReRollStatus.Start,
                                    },
                                }
                                : gameInstance
                    ),
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }

            default:
                return state;
        }
    }
}