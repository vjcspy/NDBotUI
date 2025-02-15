using Avalonia.ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainer : ReactiveUserControl<MoriConfigContainerViewModel>
{
    public MoriConfigContainer()
    {
        DataContext = new MoriConfigContainerViewModel();
        InitializeComponent();
    }
}