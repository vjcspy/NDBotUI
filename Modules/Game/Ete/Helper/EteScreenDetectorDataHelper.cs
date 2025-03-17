using System;
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

    public override Enum[] TemplateKeys { get; set; } =
        [
            EteTemplateKey.StartCloseDialogButton,
        ];

    public static EteScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}