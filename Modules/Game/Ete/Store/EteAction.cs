using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteAction
{
    public enum Type
    {
        InitEte,
    }

    public static readonly EventActionFactory InitEte = new(Type.InitEte);
}