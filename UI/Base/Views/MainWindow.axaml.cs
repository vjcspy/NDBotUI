using Avalonia.ReactiveUI;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Base.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}