using Avalonia.ReactiveUI;
using NDBotUI.UI.Emulator.ViewModels;

namespace NDBotUI.UI.Emulator.Views;

public partial class EmulatorsView : ReactiveUserControl<EmulatorsViewModel>
{
    public EmulatorsView()
    {
        DataContext = new EmulatorsViewModel();
        InitializeComponent();
    }
}