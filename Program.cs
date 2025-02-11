using Avalonia;
using System;
using System.Collections.Generic;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.Modules.Shared.TedBed.RxEvent;

namespace NDBotUI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        List<object> baseEffects = [new TedBedEffect(), new EmulatorEffect()];
        baseEffects.AddRange(MoriEffect.Effects);
        RxEventManager.RegisterEvent(baseEffects.ToArray());

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}