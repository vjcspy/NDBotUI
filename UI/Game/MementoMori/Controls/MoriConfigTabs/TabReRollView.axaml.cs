using Avalonia.ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls.MoriConfigTabs;

public partial class TabReRollView : ReactiveUserControl<TabReRollViewModel>
{
    public TabReRollView()
    {
        DataContext = new TabReRollViewModel();
        InitializeComponent();
    }
}