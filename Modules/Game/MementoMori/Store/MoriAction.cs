using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriAction
{
    public enum Type
    {
        TriggerManually,
        InitMori,
        TriggerScanCurrentScreen,
    }

    public static readonly EventActionFactory<object?> TriggerManually = new(Type.TriggerManually);
    public static readonly EventActionFactory<object?> TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
    public static readonly EventActionFactory<object?> Init = new(Type.InitMori);
}