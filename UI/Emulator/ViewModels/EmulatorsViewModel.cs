using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;
using NLog;

namespace NDBotUI.UI.Emulator.ViewModels;

public class EmulatorsViewModel : ViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    public EmulatorsViewModel()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorInitAction.Create());
    }
}