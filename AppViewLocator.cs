using System;
using NDBotUI.Modules.Core.ViewModels;
using NDBotUI.Modules.Core.Views;
using NDBotUI.ViewModels;
using NDBotUI.ViewModels.TedBed;
using NDBotUI.Views;
using NDBotUI.Views.TedBed;
using ReactiveUI;

namespace NDBotUI;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        MainWindowViewModel context => new MainWindow { DataContext = context },
        AutoContainerViewModel context => new AutoContainer { DataContext = context },

        /*TEDBED*/
        ProductPageViewModel context => new ProductPage { DataContext = context },
        ProductListViewModel context => new ProductListView { DataContext = context },
        ProductDetailViewModel context => new ProductDetailView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}