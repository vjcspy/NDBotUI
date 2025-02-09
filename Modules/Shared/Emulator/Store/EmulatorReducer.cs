using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public class EmulatorReducer
{
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
                        EmulatorConnections = list
                    };
                }


                return state;
            default:
                return state;
        }
    }
}