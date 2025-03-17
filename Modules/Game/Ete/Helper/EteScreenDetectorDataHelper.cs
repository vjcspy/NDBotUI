using System;
using System.Collections.Generic;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.Ete.Store;

namespace NDBotUI.Modules.Game.Ete.Helper;

public class EteScreenDetectorDataHelper : ScreenDetectorDataBase
{
    private static readonly EteScreenDetectorDataHelper Instance = new();

    private EteScreenDetectorDataHelper()
    {
    }

    protected override string FolderPath { get => @"Resources\game\ete\screen-detector"; }

    public override Dictionary<Enum, OverrideScreenData> OverrideScreen
    {
        get => new()
        {
            { EteTemplateKey.GuideEnemy2Assault, new OverrideScreenData(Priority: 80, FilePath: null) },
            { EteTemplateKey.GuideEnemy2Assault2, new OverrideScreenData(Priority: 80, FilePath: null) },
            { EteTemplateKey.GuideSupplyQuick, new OverrideScreenData(Priority: 50, FilePath: null) },
            { EteTemplateKey.GuideSupplyFree, new OverrideScreenData(Priority: 80, FilePath: null) },
            { EteTemplateKey.ConfirmButton, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.ConfirmButton1, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.GuideSupplyBack, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.GuideContinueBattle, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.Mission1Header, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.Mission1Task, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.MissionStartBtn, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.CloseDailyRwBttn, new OverrideScreenData(Priority: 70, FilePath: null) },
            { EteTemplateKey.Supply10xTextConfirm, new OverrideScreenData(Priority: 70, FilePath: null) },
        };
    }

    public override Enum[] TemplateKeys { get; set; } =
        [
            EteTemplateKey.StartCloseDialogButton,
            EteTemplateKey.BackBtn,
            EteTemplateKey.LoginAgreementText,
            EteTemplateKey.GraphicQualityText,
            EteTemplateKey.SkipButton,
            EteTemplateKey.ClickStrike,
            EteTemplateKey.GuideJoystick,
            EteTemplateKey.GuideNormalAttk,
            EteTemplateKey.GuideIntrinsicSkill,
            EteTemplateKey.InBattleCharIcon1,
            EteTemplateKey.InBattleCharIcon11,
            EteTemplateKey.GuideEnemy2Assault,
            EteTemplateKey.GuideEnemy2Assault2,
            EteTemplateKey.GuideNoPrblText,
            EteTemplateKey.GuideEnterName,

            EteTemplateKey.HomeSupplyBtn,
            EteTemplateKey.GuideSupplyQuick,
            EteTemplateKey.SupplyBannerStandard,
            EteTemplateKey.GuideSupplyFree,
            EteTemplateKey.ConfirmButton,
            EteTemplateKey.ConfirmButton1,
            EteTemplateKey.GuideSupplyBack,

            // home step 2
            EteTemplateKey.GuideContinueBattle,
            EteTemplateKey.Mission1Header,
            EteTemplateKey.Mission1Task,
            EteTemplateKey.MissionStartBtn,
            EteTemplateKey.CloseDailyRwBttn,

            EteTemplateKey.EmailClaimAllBtn,
            EteTemplateKey.BannerCharDragon,
            EteTemplateKey.Supply10xTextConfirm,
            EteTemplateKey.SupplySkipText,
            EteTemplateKey.SupplyExtraText,
        ];

    public static EteScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}