using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Game.R1999.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Reducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static R1999State Reduce(R1999State state, EventAction action)
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
                                    R1999GameInstance.Factory(emulatorConnection.Id)
                                ),
                            };
                        }
                    }
                }

                return state;
            }

            case R1999Action.Type.ToggleStartStopReRoll:
            {
                if (action.Payload is not BaseActionPayload baseActionPayload)
                {
                    return state;
                }

                var emulatorId = baseActionPayload.EmulatorId;

                var isRunning = state.IsReRollJobRunning(emulatorId);
                var newJobReRollState = R1999JobReRollState.Factory();
                state = state with
                {
                    GameInstances = state.GameInstances.Map(
                        gameInstance =>
                            gameInstance.EmulatorId == emulatorId
                                ? gameInstance with
                                {
                                    State = isRunning ? AutoState.Off : AutoState.On,
                                    JobType = R1999JobType.ReRoll,
                                    JobReRollState = newJobReRollState with
                                    {
                                        ReRollStatus = isRunning ? R1999ReRollStatus.Open : R1999ReRollStatus.Start,
                                    },
                                }
                                : gameInstance
                    ),
                };
                TemplateImageDataHelper.ResetTemplateImagesPriority(emulatorId);
                return state;
            }
        }

        return state;
    }
}