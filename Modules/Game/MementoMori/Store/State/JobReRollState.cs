namespace NDBotUI.Modules.Game.MementoMori.Store.State;

public enum ReRollStatus
{
    Open, // chưa làm gì hết
    Start, // Bấm start

    EligibilityChapterCheck, // Check chapter level
    IneligibleChapterCheck,
    
    EligibilityLevelCheck,
    
    Processing,
    Finished,

    ErrorCouldNotDetectCurrentScreen,

    StoppedWithError
}

public record JobReRollState(
    ReRollStatus ReRollStatus = ReRollStatus.Open,
    MoriTemplateKey MoriCurrentScreen = MoriTemplateKey.Unknown,
    MoriTemplateKey MoriLastScreen = MoriTemplateKey.Unknown,
    int DetectScreenTry = 0
);