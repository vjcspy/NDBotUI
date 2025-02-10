using Avalonia.ReactiveUI;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Base.Views;

public partial class AutoContainer : ReactiveUserControl<AutoContainerViewModel>
{
    public AutoContainer()
    {
        InitializeComponent();
    }
}