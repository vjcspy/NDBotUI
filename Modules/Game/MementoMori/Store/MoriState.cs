using System.Collections.Generic;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public record GameInstance(string ConnectionId);

public record MoriState(List<GameInstance> GameInstances)
{
    public static MoriState factory()
    {
        return new MoriState([]);
    }
}