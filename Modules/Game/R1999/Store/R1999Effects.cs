using NDBotUI.Modules.Game.R1999.Store.Effects;
using NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Effects
{
    public static readonly object[] Effects =
    [
        new InitR1999Effect(),

        new DetectCurrentScreenEffect(),
        new WhenDetectedScreenQuestEffect(),
        new WhenCouldNotDetectScreenEffect(),
        new WhenDetectedScreenSummonEffect(),
        new WhenDetectedScreenSaveResultEffect(),
    ];
}