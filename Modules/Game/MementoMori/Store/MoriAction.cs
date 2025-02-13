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

        EligibilityCheck,

        TriggerScanCurrentScreen,
    }

    public static readonly EventActionFactory TriggerManually = new(Type.TriggerManually);

    /*
     * Khi vào Mori Container, hay nói cách khác là chọn auto game Mori thì sẽ call action này ở View
     */
    public static readonly EventActionFactory InitMori = new(Type.InitMori);

    public static readonly EventActionFactory InitMoriSuccess = new(Type.InitMoriSuccess);
    /* __________________ */

    public static readonly EventActionFactory ToggleStartStopMoriReRoll = new(Type.ToggleStartStopMoriReRoll);

    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);

    public static readonly EventActionFactory EligibilityCheck = new(Type.EligibilityCheck);
}