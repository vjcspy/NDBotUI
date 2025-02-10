using Avalonia.ReactiveUI;
using NDBotUI.UI.TedBed.ViewModels;

namespace NDBotUI.UI.TedBed.Views;

public partial class ProductListView : ReactiveUserControl<ProductListViewModel>
{
    public ProductListView()
    {
        InitializeComponent();
    }
}