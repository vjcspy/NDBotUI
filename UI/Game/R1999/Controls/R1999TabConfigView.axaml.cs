using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999TabConfigView : ReactiveUserControl<R1999TabConfigViewModel>
{
    public R1999TabConfigView()
    {
        DataContext = new R1999TabConfigViewModel();
        InitializeComponent();
    }

}