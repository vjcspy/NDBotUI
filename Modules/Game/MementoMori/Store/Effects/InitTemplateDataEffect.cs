﻿using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class InitTemplateDataEffect
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static EventAction<object?> Process(EventAction<object?> action)
    {
        TemplateImageDataHelper.LoadTemplateImages();
        return CorAction.Empty;
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream.OfAction([MoriAction.Init]).Select(Process);
    }
}