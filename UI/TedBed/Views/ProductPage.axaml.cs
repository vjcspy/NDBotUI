using Avalonia.ReactiveUI;
using ProductPageViewModel = NDBotUI.UI.TedBed.ViewModels.ProductPageViewModel;

namespace NDBotUI.UI.TedBed.Views;

public partial class ProductPage : ReactiveUserControl<ProductPageViewModel>
{
    public ProductPage()
    {
        InitializeComponent();
    }
}