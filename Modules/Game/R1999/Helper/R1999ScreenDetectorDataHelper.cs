using System;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.R1999.Store;

namespace NDBotUI.Modules.Game.R1999.Helper;

public class R1999ScreenDetectorDataHelper : ScreenDetectorDataBase
{
    private static readonly R1999ScreenDetectorDataHelper Instance = new(); // Lưu instance
    private R1999ScreenDetectorDataHelper() { }
    protected override string FolderPath { get => @"Resources\game\r1999\screen-detector"; }
    public override Enum[] TemplateKeys { get; set; } = [
        R1999TemplateKey.SkipMovieBtn1,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.TapAnywhereToClose,
    ];

    public static R1999ScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}