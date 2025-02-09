using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public static class EmulatorAction
{
    private const string EMULATOR_INIT = "EMULATOR_INIT";

    public static EventActionFactory<object?> EMULATOR_INIT_ACTION = new(EMULATOR_INIT);
}