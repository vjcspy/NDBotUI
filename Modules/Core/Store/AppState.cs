namespace NDBotUI.Modules.Core.Store;

public enum Game
{
    R1999,
    MementoMori,
    EteChronicle
}

public record AppState(string AppName, Game Game, int Count)
{
    public static AppState factory()
    {
        return new AppState("NDBot", Game.EteChronicle, 0);
    }
}