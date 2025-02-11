using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store;

public class MoriAction
{
    public enum Type
    {
        TriggerScanCurrentScreen,
    }
    
    public static EventActionFactory<object?> TriggerScanCurrentScreen = new(Type.TriggerScanCurrentScreen);
}