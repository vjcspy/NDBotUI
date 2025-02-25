using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store;

public class R1999Reducer
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static R1999State Reduce(R1999State state, EventAction action)
    {
        return state;
    }
}