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
    StartCloseDialogButton,
}

public enum EteReRollStatus
{
    Open = 0, // chưa làm gì hết
    Start, // Bấm start
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