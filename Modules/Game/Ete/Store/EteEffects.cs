using NDBotUI.Modules.Game.Ete.Store.Effects;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteEffects
{
    public static readonly object[] Effects =
    [
        new InitEteEffect(),
        new DetectCurrentScreenEffect(),
    ];
}