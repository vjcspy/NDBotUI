using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.ViewModels;

namespace NDBotUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}