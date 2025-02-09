using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Shared.Emulator.ViewModels;

namespace NDBotUI.Modules.Shared.Emulator.Views;

public partial class EmulatorsView : ReactiveUserControl<EmulatorsViewModel>
{
    public EmulatorsView()
    {
        DataContext = new EmulatorsViewModel();
        InitializeComponent();
    }
}