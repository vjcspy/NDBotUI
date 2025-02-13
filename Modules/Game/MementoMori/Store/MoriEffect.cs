using System.Collections.Generic;
using NDBotUI.Modules.Game.MementoMori.Store.Effects;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriEffect
{
    public static readonly object[] Effects =
    [
        new InitTemplateDataEffect(), new EffectTemplate(), new InitAutoInstanceWhenSelectEmulator()
    ];
}