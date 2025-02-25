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
}

public record R1999GameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    R1999JobType JobType,
    JobReRollState JobReRollState
)
{
    public static R1999GameInstance Factory(string emulatorId)
    {
        return new R1999GameInstance(
            emulatorId,
            AutoState.Off,
            "",
            R1999JobType.None,
            JobReRollState.Factory()
        );
    }
}

public record R1999JobReRollState(
    ReRollStatus ReRollStatus,
    R1999TemplateKey CurrentScreenTemplate,
    R1999TemplateKey LastScreenTemplate,
    int DetectScreenTry,
    Guid? ResultId
)
{
    public static R1999JobReRollState Factory()
    {
        return new R1999JobReRollState(
            ReRollStatus.Open,
            R1999TemplateKey.Unknown,
            R1999TemplateKey.Unknown,
            0,
            null
        );
    }
}

public class R1999State(Lst<R1999GameInstance> gameInstances)
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
            return gameInstances.First(g => g.EmulatorId == emulatorId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool IsReRollJobRunning(string emulatorId)
    {
        return gameInstances
            .Find(instance => instance.EmulatorId == emulatorId)
            .Map(gameInstance => gameInstance.State == AutoState.On)
            .Match(x => x, () => false);
    }
}