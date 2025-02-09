using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Shared.Emulator.ViewModels;

namespace NDBotUI.Modules.Shared.Emulator.Views;

public partial class Emulators : ReactiveUserControl<EmulatorsVM>
{
    public Emulators()
    {
        InitializeComponent();
    }
}