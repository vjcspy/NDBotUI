using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using NDBotUI.ViewModels.TedBed;

namespace NDBotUI.Views.TedBed;

public partial class ProductPage : ReactiveUserControl<ProductPageViewModel>
{
    public ProductPage()
    {
        InitializeComponent();
    }
}