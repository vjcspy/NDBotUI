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
        };
    }

    public override Enum[] TemplateKeys { get; set; } =
        [
            EteTemplateKey.StartCloseDialogButton,
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
        ];

    public static EteScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}