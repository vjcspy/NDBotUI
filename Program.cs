using Avalonia;
using System;
using Avalonia.ReactiveUI;
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
        RxEventManager.RegisterEvent(new TedBedEffect());
        RxEventManager.Dispatch(TestBedAction.FOO_ACTION.Create(null));

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