using System;
using System.Linq;
using LanguageExt;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public record GameInstance(
    string EmulatorId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    MoriJobType JobType,
    JobReRollState JobReRollState
);

public record MoriState(Lst<GameInstance> GameInstances)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static MoriState Factory()
    {
        return new MoriState([]);
    }

    public GameInstance? GetGameInstance(string emulatorId)
    {
        try
        {
            return GameInstances.First(g => g.EmulatorId == emulatorId);
        }
        catch (Exception)
        {
            Logger.Warn($"Could not find emulator with id {emulatorId}");
            return null;
        }
    }

    public GameInstance? GetCurrentEmulatorGameInstance()
    {
        if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
            return GetGameInstance(selectedEmulatorId);

        return null;
    }

    public bool IsReRollJobRunning(string emulatorId)
    {
        return GameInstances
            .Find(instance => instance.EmulatorId == emulatorId)
            .Map(gameInstance => gameInstance.State == AutoState.On)
            .Match(x => x, () => false);
    }
}