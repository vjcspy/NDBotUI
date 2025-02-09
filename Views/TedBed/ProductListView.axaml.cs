using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NDBotUI.ViewModels.TedBed;

namespace NDBotUI.Views.TedBed;

public partial class ProductListView : ReactiveUserControl<ProductListViewModel>
{
    public ProductListView()
    {
        InitializeComponent();
    }
}