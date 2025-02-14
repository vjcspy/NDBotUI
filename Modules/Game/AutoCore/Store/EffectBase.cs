using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.AutoCore.Store;

public abstract class EffectBase
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    protected virtual bool GetForceEligible()
    {
        return false;
    }

    protected abstract IEventActionFactory[] GetAllowEventActions();
    protected abstract Task<EventAction> Process(EventAction action);


    [Effect]
    public virtual RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(GetAllowEventActions())
            .FilterBaseEligibility(GetForceEligible())
            .SelectMany(Process);
    }
}