namespace NDBotUI.Modules.Core.Store;

public record AppState(string AppName, int Count)
{
    public static AppState factory()
    {
        return new AppState("NDBot", 0);
    }
}