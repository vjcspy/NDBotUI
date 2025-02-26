using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Action
{
    public enum Type
    {
        InitR1999,

        TriggerScanCurrentScreen,
        DetectScreen,

        ClickedAfterDetectedScreen,
        ToggleStartStopReRoll
    }

    public static readonly EventActionFactory InitR1999 = new(Type.InitR1999);

    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
    public static readonly EventActionFactory DetectScreen = new(Type.DetectScreen);

    public static readonly EventActionFactory ClickedAfterDetectedScreen = new(Type.ClickedAfterDetectedScreen);
    public static readonly EventActionFactory ToggleStartStopReRoll = new(Type.ToggleStartStopReRoll);
}