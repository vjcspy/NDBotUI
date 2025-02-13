using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                if (action.Payload is BaseActionPayload baseActionPayload)
                {
                    Logger.Info($"Selected Emulator : {baseActionPayload.EmulatorId}");
                    state = state with
                    {
                        SelectedEmulatorId = baseActionPayload.EmulatorId,
                    };
                }


                return state;
            default:
                return state;
        }
    }
}