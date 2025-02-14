using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Core.Attributes;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.Modules.Shared.TedBed.RxEvent;

namespace NDBotUI;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RegisterEffects();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    [SingleCall]
    private static void RegisterEffects()
    {
        List<object> baseEffects = [new TedBedEffect(), new EmulatorEffect()];
        baseEffects.AddRange(MoriEffect.Effects);
        RxEventManager.RegisterEvent(baseEffects.ToArray());
    }
}