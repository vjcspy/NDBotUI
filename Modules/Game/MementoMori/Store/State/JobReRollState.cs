using System;

namespace NDBotUI.Modules.Game.MementoMori.Store.State;

public enum ReRollStatus
{
    Open = 0, // chưa làm gì hết
    Start, // Bấm start

    EligibilityChapterCheck, // Check chapter level
    EligibilityChapterPassed, // Check chapter level

    EligibilityLevelCheck, // Đang checking level
    EligibilityLevelCheckOnChar, // Đang checking level
    EligibilityLevelPassed,

    NextChapter,
    SaveResult,
    ResetUserData,

    StoppedWithError,
}

public record JobReRollState(
    ReRollStatus ReRollStatus,
    MoriTemplateKey CurrentScreenTemplate,
    MoriTemplateKey LastScreenTemplate,
    int DetectScreenTry,
    int CurrentLevel,
    int LevelUpCharPosition,
    Guid? ResultId
)
{
    public static JobReRollState Factory()
    {
        return new JobReRollState(
            ReRollStatus.Open,
            MoriTemplateKey.Unknown,
            MoriTemplateKey.Unknown,
            0,
            0,
            0,
            null
        );
    }
}