using System;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.R1999.Store;

namespace NDBotUI.Modules.Game.R1999.Helper;

public class ScreenDetectorDataHelper : ScreenDetectorDataBase
{
    protected override string FolderPath { get => @"Resources\r1999\screen-detector"; }
    public override Enum[] TemplateKeys { get; set; } = [R1999TemplateKey.Unknown,];
}