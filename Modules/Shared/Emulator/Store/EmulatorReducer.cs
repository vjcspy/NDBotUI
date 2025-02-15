using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorReducer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static EmulatorState Reduce(EmulatorState state, EventAction action)
    {
        switch (action.Type)
        {
            case EmulatorAction.Type.EmulatorConnectSuccess:
            {
                if (action.Payload is Lst<EmulatorConnection> list)
                {
                    var list1Keys = list.Map(e=> e.Id).ToHashSet();
                    var list2Keys = state.EmulatorConnections.Map(e=> e.Id).ToHashSet();
                    
                    var isEqual = list1Keys.SetEquals(list2Keys);

                    if (!isEqual)
                    {
                        Console.WriteLine($"Emulator Updated List: {list.Count}");
                        state = state with
                        {
                            EmulatorConnections = list,
                            IsLoaded = true,
                            Attempts = 0
                        };
                    }
                }


                return state;
            }

            case EmulatorAction.Type.EmulatorConnectError:
            {
                state = state with
                {
                    Attempts = state.Attempts + 1,
                };

                return state;
            }

            case EmulatorAction.Type.SelectEmulatorConnection:
            {
                if (action.Payload is BaseActionPayload baseActionPayload)
                {
                    Logger.Info($"Selected Emulator : {baseActionPayload.EmulatorId}");
                    state = state with
                    {
                        SelectedEmulatorId = baseActionPayload.EmulatorId,
                    };
                }


                return state;
            }
            default:
                return state;
        }
    }
}