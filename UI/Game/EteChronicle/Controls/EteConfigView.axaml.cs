using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteConfigView : UserControl
{
    public EteConfigView()
    {
        DataContext = new EteTabConfigViewModel();
        InitializeComponent();
    }
}