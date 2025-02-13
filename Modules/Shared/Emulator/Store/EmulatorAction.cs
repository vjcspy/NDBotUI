using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public static class EmulatorAction
{
    public enum Type
    {
        EmulatorInit,
        EmulatorConnectSuccess,
        EmulatorConnectError,

        SelectEmulatorConnection,
    }

    public static readonly EventActionFactory EmulatorInitAction = new(Type.EmulatorInit);

    public static readonly EventActionFactory EmulatorConnectSuccessAction =
        new(Type.EmulatorConnectSuccess);

    public static readonly EventActionFactory EmulatorConnectError =
        new(Type.EmulatorConnectError);

    public static readonly EventActionFactory SelectEmulatorConnection =
        new(Type.SelectEmulatorConnection);
}