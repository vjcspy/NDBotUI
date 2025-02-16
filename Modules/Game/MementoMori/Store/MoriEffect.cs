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
        new OnDetectedTemplateEffectReRoll(),
        new WhenFoundCharacterGrowthPossible(),
        new SpamClickWhenCouldNotDetect(),
        
        /* After roll*/
        new SaveResultEffect(),
        new ResetUserDataEffect(),
    ];
}