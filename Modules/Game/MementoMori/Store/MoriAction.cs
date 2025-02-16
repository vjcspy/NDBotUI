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

        EligibilityChapterCheck,
        EligibilityLevelCheck,
        EligibilityLevelCheckError,
        EligibilityLevelPass,

        TriggerScanCurrentScreen,
        DetectedMoriScreen,
        ClickedAfterDetectedMoriScreen,
        CouldNotDetectMoriScreen,

        ResetUserData,
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

    public static readonly EventActionFactory EligibilityChapterCheck = new(Type.EligibilityChapterCheck);
    public static readonly EventActionFactory EligibilityLevelCheck = new(Type.EligibilityLevelCheck);
    public static readonly EventActionFactory EligibilityLevelCheckError = new(Type.EligibilityLevelCheckError);
    public static readonly EventActionFactory EligibilityLevelPass = new(Type.EligibilityLevelPass);

    public static readonly EventActionFactory DetectedMoriScreen = new(Type.DetectedMoriScreen);
    public static readonly EventActionFactory CouldNotDetectMoriScreen = new(Type.CouldNotDetectMoriScreen);
    public static readonly EventActionFactory ClickedAfterDetectedMoriScreen = new(Type.ClickedAfterDetectedMoriScreen);


    public static readonly EventActionFactory ResetUserData = new(Type.ResetUserData);
}