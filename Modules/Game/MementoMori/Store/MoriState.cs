using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public enum MoriTemplateKey
{
    /*Level up*/
    BeforeChallengeChapterSix, // check chapter nay se nang map lv7 het
    BeforeChallengeEnemyPower15 = 15, // 
    BeforeChallengeEnemyPower16 = 16, //
    BeforeChallengeEnemyPower17 = 17, //
    BeforeChallengeEnemyPower18 = 18, //
    BeforeChallengeEnemyPower19 = 19, //
    BeforeChallengeEnemyPower111 = 111, //
    BeforeChallengeEnemyPower112 = 112, //
    BeforeChallengeEnemyPower21 = 221, //
    BeforeChallengeEnemyPower22 = 222, //

    Unknown = 500,
    TermOfAgreementPopup,
    StartSettingButton,
    StartStartButton,

    IconChar1,

    ChallengeButton, // Trước khi vào trận đánh sẽ hỏi
    TextSelectFirstCharToTeam, // Select first char vào party
    TextSelectSecondCharToTeam, // Select second char vào party
    TextSelectThirdCharToTeam, // Select third char vào party
    TextSelectFourCharToTeam, // Select four char vào party

    PowerLevelUpText, // hướng dẫn level up nhân vật
    GuideClickLevelUpText, // click level up nhân vật
    GuideClickEquipAllText, // click level up nhân vật
    GuideClickQuestText, // click level up nhân vật
    GuideClickTheTownText, // click level up nhân vật
    GuideSelectTownButton, // click level up nhân vật
    GuideClickRewardText, // click vào thùng reward
    GuideClickLevelUpImmediatelyText,
    GuideClickHomeText,
    GuideClickDownButton, // Button Hướng dẫn Click Down
    GuideChapter12Text,
    GuideChapter12Text1, // do cai này khó nên cần 1 vài ảnh khác nhau

    BossBattleButton,
    SelectButton, // button select ở bottom right (khi chọn town)
    ButtonClaim, // button claim reward
    NextCountryButton,
    NextChapterButton,

    /* Level UP*/
    CharacterGrowthPossible,
    CharacterGrowthTabHeader,
    CharacterLevelOneText,
    CharacterLevelTwoText,
    CharacterLevelThreeText,
    CharacterLevelFourText,
    CharacterLevelFiveText,
    CharacterLevelSixText,
    CharacterLevelSevenText,

    PartyInformation,
    TapToClose, // sau khi battle kết thúc

    SkipMovieButton,
    SkipSceneShotButton,
    HomeNewPlayerText,

    /*Save result*/
    CharacterTabHeader,
    ReturnToTitleButton,
    ReturnToTitleHeader, // cần confirm

    /*In battle*/
    InBattleX2,
    InBattleX1,

    /*Home*/
    LoginClaimButton,
    HomeIconBpText, // dùng để detect đang ở home
    
    /*Reset*/
    ResetGameDataButton,
    ResetGameDataHeader, // confirm
    ConfirmGameDataResetHeader, // confirm
}

public record GameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    MoriJobType JobType,
    JobReRollState JobReRollState
)
{
    public static GameInstance Factory(string emulatorId)
    {
        return new GameInstance(
            emulatorId,
            AutoState.Off,
            "",
            MoriJobType.None,
            JobReRollState.Factory()
        );
    }
}

public record MoriState(Lst<GameInstance> GameInstances)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static MoriState Factory()
    {
        return new MoriState([]);
    }

    public GameInstance? GetGameInstance(string emulatorId)
    {
        try
        {
            return GameInstances.First(g => g.EmulatorId == emulatorId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool IsReRollJobRunning(string emulatorId)
    {
        return GameInstances
            .Find(instance => instance.EmulatorId == emulatorId)
            .Map(gameInstance => gameInstance.State == AutoState.On)
            .Match(x => x, () => false);
    }
}