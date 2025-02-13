using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public static class EmulatorAction
{
    public enum Type
    {
        EmulatorInit,
        EmulatorConnectSuccess,
        SelectEmulatorConnection,
    }

    public static readonly EventActionFactory EmulatorInitAction = new(Type.EmulatorInit);

    public static readonly EventActionFactory EmulatorConnectSuccessAction =
        new(Type.EmulatorConnectSuccess);
    
    public static readonly EventActionFactory SelectEmulatorConnection =
        new(Type.SelectEmulatorConnection);
}