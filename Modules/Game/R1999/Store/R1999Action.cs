using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Action
{
    public enum Type
    {
        InitR1999,

        TriggerScanCurrentScreen,
        DetectScreen,
        CouldNotDetectScreen,

        ClickedAfterDetectedScreen,
        ToggleStartStopReRoll,

        GotDailyReward,
        GotChapterReward,
        GotEmailReward,
        RollX1,
        RollFinished,
        SaveResultOk,

        // register account
        ClickedSendCode,
        SentCode,
        RegisteredAccount,
    }

    public static readonly EventActionFactory InitR1999 = new(Type.InitR1999);

    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
    public static readonly EventActionFactory DetectScreen = new(Type.DetectScreen);
    public static readonly EventActionFactory CouldNotDetectScreen = new(Type.CouldNotDetectScreen);

    public static readonly EventActionFactory ClickedAfterDetectedScreen = new(Type.ClickedAfterDetectedScreen);
    public static readonly EventActionFactory ToggleStartStopReRoll = new(Type.ToggleStartStopReRoll);
    public static readonly EventActionFactory GotDailyReward = new(Type.GotDailyReward);
    public static readonly EventActionFactory GotChapterReward = new(Type.GotChapterReward);
    public static readonly EventActionFactory GotEmailReward = new(Type.GotEmailReward);
    public static readonly EventActionFactory RollX1 = new(Type.RollX1);
    public static readonly EventActionFactory RollFinished = new(Type.RollFinished);
    public static readonly EventActionFactory SaveResultOk = new(Type.SaveResultOk);

    public static readonly EventActionFactory ClickedSendCode = new(Type.ClickedSendCode);
    public static readonly EventActionFactory SentCode = new(Type.SentCode);
    public static readonly EventActionFactory RegisteredAccount = new(Type.RegisteredAccount);
}