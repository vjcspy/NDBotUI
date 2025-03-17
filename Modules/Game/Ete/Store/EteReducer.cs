using LanguageExt;
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

            default:
                return state;
        }
    }
}