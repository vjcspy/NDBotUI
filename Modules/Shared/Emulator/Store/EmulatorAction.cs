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

    public static readonly EventActionFactory<object?> EmulatorInitAction = new(Type.EmulatorInit);

    public static readonly EventActionFactory<object?> EmulatorConnectSuccessAction =
        new(Type.EmulatorConnectSuccess);
    
    public static readonly EventActionFactory<object?> SelectEmulatorConnection =
        new(Type.SelectEmulatorConnection);
}