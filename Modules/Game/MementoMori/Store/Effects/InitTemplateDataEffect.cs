using System.Reactive.Linq;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class InitTemplateDataEffect
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static EventAction Process(EventAction action)
    {
        // Init template for scanning image
        TemplateImageDataHelper.LoadTemplateImages();

        return MoriAction.InitMoriSuccess.Create();
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(MoriAction.InitMori)
            .Select(Process);
    }
}