using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.Ete.Typing;
using NDBotUI.Modules.Game.R1999.Typing;
using NLog;

namespace NDBotUI.Modules.Game.Ete.Store;

public enum EteTemplateKey
{
    Unknown,
    BackBtn,
    StartCloseDialogButton,
    LoginAgreementText,
    GraphicQualityText,
    SkipButton,
    ClickStrike,
    GuideJoystick,
    GuideNormalAttk,

    GuideIntrinsicSkill,
    InBattleCharIcon1,
    InBattleCharIcon11,
    GuideEnemy2Assault,
    GuideEnemy2Assault2,

    GuideNoPrblText,
    GuideEnterName,

    HomeSupplyBtn,
    GuideSupplyQuick,
    SupplyBannerStandard,
    GuideSupplyFree,
    ConfirmButton,
    ConfirmButton1,
    GuideSupplyBack,

    GuideContinueBattle,
    Mission1Header,
    Mission1Task,
    MissionStartBtn,
    CloseDailyRwBttn,

    EmailClaimAllBtn,
    BannerCharDragon,
    Supply10xTextConfirm,
    SupplySkipText,
    SupplyExtraText,
    SupplyConfirmBtn,
    SupplyTapEmpty,
    SupplyConsum1000Text,
    LackMoneyText,
    CloseXBtn,
    CharacterIconScreen,
    ReplenishDailyBtn,
}

public enum EteReRollStatus
{
    Open = 0, // chưa làm gì hết
    Start, // Bấm start

    Step1HomeScreen, // Xong mở đầu, vào được home
    Step2HomeScreen, // Xong mở đầu, vao battle lan nua
    Step3HomeScreen, // Xong mở đầu, Done battle

    GetReward,
    GotEmailReward,
    GotCodelReward,

    DoReRoll,
    DoReRollWeaponOk,
    DoReRollLackMoneyX10,
    DoReRollCharOk,
}

public record EteJobReRollState(
    EteReRollStatus ReRollStatus,
    CurrentScreen CurrentScreen,
    int DetectScreenTry,
    string Ordinal
)
{
    public static EteJobReRollState Factory()
    {
        return new EteJobReRollState(
            EteReRollStatus.Open,
            new CurrentScreen(),
            0,
            ""
        );
    }
}

public record EteGameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    EteJobType JobType,
    EteJobReRollState JobReRollState
)
{
    public static EteGameInstance Factory(string emulatorId)
    {
        return new EteGameInstance(
            emulatorId,
            AutoState.Off,
            "",
            EteJobType.None,
            EteJobReRollState.Factory()
        );
    }
}

public record EteState(Lst<EteGameInstance> GameInstances)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static EteState Factory()
    {
        return new EteState([]);
    }

    public EteGameInstance? GetGameInstance(string emulatorId)
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