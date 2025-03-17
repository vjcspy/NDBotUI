using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteAction
{
    public enum Type
    {
        InitEte,
        ToggleStartStopReRoll,

        TriggerScanCurrentScreen,
        DetectScreen,
        CouldNotDetectScreen,
        ClickedAfterDetectedScreen,

        GetReward,
        GotEmailReward,
        GotCodeReward,

        DoReRoll
    }

    public static readonly EventActionFactory InitEte = new(Type.InitEte);
    public static readonly EventActionFactory ToggleStartStopReRoll = new(Type.ToggleStartStopReRoll);

    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
    public static readonly EventActionFactory DetectScreen = new(Type.DetectScreen);
    public static readonly EventActionFactory CouldNotDetectScreen = new(Type.CouldNotDetectScreen);
    public static readonly EventActionFactory ClickedAfterDetectedScreen = new(Type.ClickedAfterDetectedScreen);

    public static readonly EventActionFactory GetReward = new(Type.GetReward);
    public static readonly EventActionFactory GotEmailReward = new(Type.GotEmailReward);

    public static readonly EventActionFactory DoReRoll = new(Type.DoReRoll);
}