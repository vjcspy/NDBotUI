using Avalonia.Controls;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999ConfigView : UserControl
{
    public R1999ConfigView()
    {
        DataContext = new R1999ConfigViewModel();
        InitializeComponent();
    }
}