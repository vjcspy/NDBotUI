using NDBotUI.Modules.Game.MementoMori.Store.Effects;
using NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriEffect
{
    public static readonly object[] Effects =
    [
        new InitTemplateDataEffect(),
        new EffectTemplate(),
        new EligibilityCheckEffect(),
        new DetectCurrentScreen(),

        /* ReRoll*/
        new ReRollOnDetectedTemplateEffect(),
        new SpamClickWhenCouldNotDetect(),
        new EligibilityLevelCheck(),
        new SaveResultEffect(),
        new ResetUserDataEffect()
        
    ];
}