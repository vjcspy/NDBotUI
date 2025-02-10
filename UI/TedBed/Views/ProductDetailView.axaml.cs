using Avalonia.ReactiveUI;
using NDBotUI.UI.TedBed.ViewModels;

namespace NDBotUI.UI.TedBed.Views;

public partial class ProductDetailView : ReactiveUserControl<ProductDetailViewModel>
{
    public ProductDetailView()
    {
        InitializeComponent();
    }
}