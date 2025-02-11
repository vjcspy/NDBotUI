namespace NDBotUI.Modules.Game.MementoMori.Store.State;

public enum ReRollStatus
{
    Start,
    Initial,

    EligibilityCheck,
    Eligible,
    Ineligible,

    Processing,
    Finished,

    ErrorCouldNotDetectCurrentScreen,

    StoppedWithError
}

public enum MoriScreen
{
    Unknown,
    SkipMovie,
}

public record JobReRollState(
    ReRollStatus ReRollStatus,
    MoriScreen MoriCurrentScreen,
    MoriScreen MoriLastScreen,
    int DetectScreenTry
);