using System;
using System.Collections.Generic;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.R1999.Store;

namespace NDBotUI.Modules.Game.R1999.Helper;

public class R1999ScreenDetectorDataHelper : ScreenDetectorDataBase
{
    private static readonly R1999ScreenDetectorDataHelper Instance = new(); // Lưu instance
    private R1999ScreenDetectorDataHelper() { }
    protected override string FolderPath { get => @"Resources\game\r1999\screen-detector"; }

    public override Dictionary<Enum, OverrideScreenData> OverrideScreen
    {
        get => new()
        {
            { R1999TemplateKey.StartLoss8Button, new OverrideScreenData(Priority: 80, FilePath: null) },
            { R1999TemplateKey.StartLoss8Button2, new OverrideScreenData(Priority: 80, FilePath: null) },
            { R1999TemplateKey.HomeMail, new OverrideScreenData(Priority: 101, FilePath: null) },
            { R1999TemplateKey.StartChapter, new OverrideScreenData(Priority: 101, FilePath: null) },
            { R1999TemplateKey.Chapter3Text, new OverrideScreenData(Priority: 102, FilePath: null) },
            { R1999TemplateKey.AttackCard, new OverrideScreenData(Priority: 102, FilePath: null) },
            { R1999TemplateKey.Chapter5Text, new OverrideScreenData(Priority: 70, FilePath: null) },
            { R1999TemplateKey.ExitButton, new OverrideScreenData(Priority: 105, FilePath: null) },
            { R1999TemplateKey.LackUnilogText, new OverrideScreenData(Priority: 50, FilePath: null) },
            { R1999TemplateKey.DontHaveEnoughText, new OverrideScreenData(Priority: 50, FilePath: null) },
            { R1999TemplateKey.SentCodeBtn, new OverrideScreenData(Priority: 50, FilePath: null) },
        };
    }

    public override Enum[] TemplateKeys { get; set; } =
    [
        R1999TemplateKey.SkipMovieBtn1,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.SignHere,
        R1999TemplateKey.SelectSkill1Text,
        R1999TemplateKey.HoldSkill2Text,
        R1999TemplateKey.LinkSkillNeighboring,
        R1999TemplateKey.SkillTimekeeperText,
        R1999TemplateKey.RitualTimeKeeperText,
        R1999TemplateKey.FullyPreparedText,
        R1999TemplateKey.GuideMeUltimate,
        R1999TemplateKey.SkillUltimateText,
        R1999TemplateKey.ReturnStoryText,
        R1999TemplateKey.Story1Text,
        R1999TemplateKey.Chapter1Button,
        R1999TemplateKey.StartLoss8Button,
        R1999TemplateKey.StartLoss8Button2,
        R1999TemplateKey.AccelerateBattleText,
        R1999TemplateKey.ChooseEnemyText,
        R1999TemplateKey.SummonText,
        R1999TemplateKey.SummonWheel,
        R1999TemplateKey.CheckCrewText,
        R1999TemplateKey.HomeMail,
        R1999TemplateKey.StartChapter,
        R1999TemplateKey.Chapter3Text,
        R1999TemplateKey.SelectTargetChapter4,
        R1999TemplateKey.AttackCard,
        R1999TemplateKey.Chapter5Text,
        R1999TemplateKey.ExitButton,
        R1999TemplateKey.ClaimChapter14Button,
        R1999TemplateKey.SummonX1Text,
        R1999TemplateKey.LackUnilogText,
        R1999TemplateKey.DontHaveEnoughText,
        R1999TemplateKey.CharacterLevelText,
        R1999TemplateKey.SettingButton,
        R1999TemplateKey.LogOutExitBtn,
        R1999TemplateKey.LoginLogoutBtn,
        R1999TemplateKey.LoginAnotherAccBtn,
        R1999TemplateKey.LoginWithEmailBtn,
        R1999TemplateKey.RegisterBtn,
        R1999TemplateKey.SendCodeBtn,
        R1999TemplateKey.RegisterAccHeader,
        R1999TemplateKey.SentCodeBtn,
        R1999TemplateKey.RegisterAccPassBtn,
        R1999TemplateKey.SomeoneFamiliarText,
    ];

    public static R1999ScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}