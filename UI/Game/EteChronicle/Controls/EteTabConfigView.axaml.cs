using Avalonia.ReactiveUI;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteTabConfigView : ReactiveUserControl<EteTabConfigViewModel>
{
    public EteTabConfigView()
    {
        DataContext = new EteTabConfigViewModel();
        InitializeComponent();
    }
}