using System;
using System.Reactive;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Emulator.ViewModels;

public class EmulatorsViewModel : ViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    public ReactiveCommand<Unit, Unit> CheckData { get; } = ReactiveCommand.Create(() =>
    {
        Console.WriteLine($"Number of emulators {AppStore.Instance.EmulatorConnections.Count}");
    });

    public EmulatorsViewModel()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorInitAction.Create());
    }
}