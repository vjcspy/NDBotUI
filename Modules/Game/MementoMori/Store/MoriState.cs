using System.Collections.Generic;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public record GameInstance(
    string ConnectionId,
    AutoState State, // On/Off Auto
    string Status, // Text cho user biết đang làm gì
    MoriJobType JobType,
    JobReRollState JobReRollState
);

public record MoriState(List<GameInstance> GameInstances)
{
    public static MoriState factory()
    {
        return new MoriState([]);
    }
}