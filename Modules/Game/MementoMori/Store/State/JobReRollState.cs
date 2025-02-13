﻿namespace NDBotUI.Modules.Game.MementoMori.Store.State;

public enum ReRollStatus
{
    Open, // chưa làm gì hết
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
    ReRollStatus ReRollStatus = ReRollStatus.Open,
    MoriScreen MoriCurrentScreen = MoriScreen.Unknown,
    MoriScreen MoriLastScreen = MoriScreen.Unknown,
    int DetectScreenTry = 0
);