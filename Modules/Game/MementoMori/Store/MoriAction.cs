using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriAction
{
    public enum Type
    {
        TriggerManually,

        ToggleStartStopMoriReRoll,

        InitMori,
        InitMoriSuccess,

        TriggerScanCurrentScreen,
    }

    public static readonly EventActionFactory TriggerManually = new(Type.TriggerManually);

    public static readonly EventActionFactory ToggleStartStopMoriReRoll = new(Type.ToggleStartStopMoriReRoll);

    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
    
    public static readonly EventActionFactory Init = new(Type.InitMori);
    public static readonly EventActionFactory InitMoriSuccess = new(Type.InitMoriSuccess);
}