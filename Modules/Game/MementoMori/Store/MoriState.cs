using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Core.Store;
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
    BeforeChallengeEnemyPower19 = 19, //
    BeforeChallengeEnemyPower111 = 111, //
    BeforeChallengeEnemyPower112 = 112, //
    BeforeChallengeEnemyPower22 = 22, //

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
    GuideClickDownButton,
    GuideChapter12Text,

    BossBattleButton,
    SelectButton, // button select ở bottom right (khi chọn town)
    ButtonClaim, // button claim reward
    NextCountryButton,

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
}

public record GameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    MoriJobType JobType,
    JobReRollState JobReRollState
);

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