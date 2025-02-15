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
    Unknown,
    StartSettingButton,
    StartStartButton,

    IconChar1, // TODO: cần chuyển sang text
    
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
    GuideClickLevelUpImmediatelyText, // click vào thùng reward
    GuideClickHomeText, // click vào thùng reward
    GuideClickDownButton, // click vào thùng reward
    
    BossBattleButton,
    SelectButton, // button select ở bottom right (khi chọn town)
    ButtonClaim, // button claim reward
    
    /*Level up*/
    BeforeChallengeChapterSix, // check chapter nay se nang map lv7 het
    BeforeChallengeEnemyPower15, // 
    BeforeChallengeEnemyPower16, //
    BeforeChallengeEnemyPower17, //
    
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

    SkipMovieButton
    
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

    public GameInstance? GetCurrentEmulatorGameInstance()
    {
        if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
            return GetGameInstance(selectedEmulatorId);

        return null;
    }

    public bool IsReRollJobRunning(string emulatorId)
    {
        return GameInstances
            .Find(instance => instance.EmulatorId == emulatorId)
            .Map(gameInstance => gameInstance.State == AutoState.On)
            .Match(x => x, () => false);
    }
}