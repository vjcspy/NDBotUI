using System.Drawing;
using NDBotUI.Modules.Game.MementoMori.Store;

namespace NDBotUI.Modules.Game.MementoMori.Typing;

public enum MoriJobType
{
    None,
    ReRoll
}

public record DetectedTemplatePoint(MoriTemplateKey MoriTemplateKey, Point Point);