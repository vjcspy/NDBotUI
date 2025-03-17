using NDBotUI.Modules.Game.AutoCore.Helper;

namespace NDBotUI.Modules.Game.Ete.Helper;

public class EteScreenDetectorDataHelper:  ScreenDetectorDataBase
{

    private static EteScreenDetectorDataHelper Instance = new();

    protected override string FolderPath { get => @"Resources\game\ete\screen-detector"; }
    private EteScreenDetectorDataHelper()
    {

    }

    public static EteScreenDetectorDataHelper GetInstance()
    {
        return Instance;
    }
}