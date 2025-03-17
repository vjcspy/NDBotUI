using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store;

public class EteAction
{
    public enum Type
    {
        InitEte,
        ToggleStartStopReRoll,

        TriggerScanCurrentScreen
    }

    public static readonly EventActionFactory InitEte = new(Type.InitEte);
    public static readonly EventActionFactory ToggleStartStopReRoll = new(Type.ToggleStartStopReRoll);
    public static readonly EventActionFactory TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
}