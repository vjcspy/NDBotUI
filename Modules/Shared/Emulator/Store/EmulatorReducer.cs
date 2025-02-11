using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorReducer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static EmulatorState Reduce(EmulatorState state, EventAction<object?> action)
    {
        switch (action.Type)
        {
            case EmulatorAction.Type.EmulatorConnectSuccess:
                if (action.Payload is List<EmulatorConnection> list)
                {
                    Console.WriteLine($"Emulator Updated List: {list.Count}");
                    state = state with
                    {
                        EmulatorConnections = list,
                        IsLoaded = true
                    };
                }


                return state;

            case EmulatorAction.Type.SelectEmulatorConnection:
                if (action.Payload is EmulatorConnection emulatorConnection)
                {
                    Logger.Info($"Selected Emulator : {emulatorConnection.Id}");
                    state = state with
                    {
                        EmulatorConnection = emulatorConnection,
                    };
                }


                return state;
            default:
                return state;
        }
    }
}