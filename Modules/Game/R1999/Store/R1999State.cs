using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.R1999.Typing;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store;

public enum R1999TemplateKey
{
    Unknown,
    SkipMovieBtn1,
    ConfirmBtn,
}

public enum R1999ReRollStatus
{
    Open = 0, // chưa làm gì hết
    Start, // Bấm start
}

public record R1999GameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    R1999JobType JobType,
    R1999JobReRollState JobReRollState
)
{
    public static R1999GameInstance Factory(string emulatorId)
    {
        return new R1999GameInstance(
            emulatorId,
            AutoState.Off,
            "",
            R1999JobType.None,
            R1999JobReRollState.Factory()
        );
    }
}

public record R1999JobReRollState(
    R1999ReRollStatus ReRollStatus,
    CurrentScreen CurrentScreen,
    int DetectScreenTry,
    Guid? ResultId
)
{
    public static R1999JobReRollState Factory()
    {
        return new R1999JobReRollState(
            R1999ReRollStatus.Open,
            new CurrentScreen(),
            0,
            null
        );
    }
}

public record R1999State(Lst<R1999GameInstance> GameInstances)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static R1999State Factory()
    {
        return new R1999State([]);
    }

    public R1999GameInstance? GetGameInstance(string emulatorId)
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