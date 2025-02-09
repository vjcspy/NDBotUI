using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.ViewModels;
using ReactiveUI;

namespace NDBotUI.Modules.Shared.Emulator.ViewModels;

public class EmulatorsViewModel : ViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    public ReactiveCommand<Unit, Unit> CheckData { get; } = ReactiveCommand.Create(() =>
    {
        Console.WriteLine($"Number of emulators {AppStore.Instance.EmulatorConnections.Count}");
    });

    public EmulatorsViewModel()
    {
    }
}