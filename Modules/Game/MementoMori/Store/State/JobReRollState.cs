namespace NDBotUI.Modules.Game.MementoMori.Store.State;

public enum ReRollStatus
{
    Open = 0, // chưa làm gì hết
    Start, // Bấm start

    EligibilityChapterCheck, // Check chapter level
    EligibilityChapterPassed, // Check chapter level

    EligibilityLevelCheck,
    EligibilityLevelPass,

    SaveResult,
    ResetUserData,

    StoppedWithError
}

public record JobReRollState(
    ReRollStatus ReRollStatus = ReRollStatus.Open,
    MoriTemplateKey MoriCurrentScreen = MoriTemplateKey.Unknown,
    MoriTemplateKey MoriLastScreen = MoriTemplateKey.Unknown,
    int DetectScreenTry = 0,
    int CurrentLevel = 0
)
{
    public static JobReRollState Factory()
    {
        return new JobReRollState();
    }
};