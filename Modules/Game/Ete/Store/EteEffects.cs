using NDBotUI.Modules.Game.Ete.Store.Effects;
using NDBotUI.Modules.Game.Ete.Store.Effects.ReRollEffects;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteEffects
{
    public static readonly object[] Effects =
    [
        new InitEteEffect(),
        new DetectCurrentScreenEffect(),

        new ReRollStartEffect(),
        new CouldNotFoundScreenEffect(),
        new ReRollHomeStep1Effect(),
        new ReRollHomeStep2Effect(),
        new ReRollHomeStep3Effect(),
        new ReRollGetRewardEffect(),
        new DoReRollEffect(),
    ];
}