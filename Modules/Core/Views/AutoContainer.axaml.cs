using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Core.ViewModels;

namespace NDBotUI.Modules.Core.Views;

public partial class AutoContainer : ReactiveUserControl<AutoContainerViewModel>
{
    public AutoContainer()
    {
        InitializeComponent();
    }
}