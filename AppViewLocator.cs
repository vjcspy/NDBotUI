using System;
using NDBotUI.UI.Base.ViewModels;
using NDBotUI.UI.Base.Views;
using NDBotUI.UI.Game.MementoMori.Controls;
using NDBotUI.UI.Game.R1999.Controls;
using NDBotUI.UI.TedBed.ViewModels;
using ReactiveUI;
using MainWindow = NDBotUI.UI.Base.Views.MainWindow;
using ProductDetailView = NDBotUI.UI.TedBed.Views.ProductDetailView;
using ProductListView = NDBotUI.UI.TedBed.Views.ProductListView;
using ProductPage = NDBotUI.UI.TedBed.Views.ProductPage;
using ProductPageViewModel = NDBotUI.UI.TedBed.ViewModels.ProductPageViewModel;

namespace NDBotUI;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null)
    {
        return viewModel switch
        {
            MainWindowViewModel context => new MainWindow { DataContext = context, },
            AutoContainerViewModel context => new AutoContainer { DataContext = context, },

            /*TEDBED*/
            ProductPageViewModel context => new ProductPage { DataContext = context, },
            ProductListViewModel context => new ProductListView { DataContext = context, },
            ProductDetailViewModel context => new ProductDetailView { DataContext = context, },

            // MORI
            MoriContainerViewModel context => new MoriContainer { DataContext = context, },

            // R1999
            R1999ContainerViewModel context => new R1999ContainerView { DataContext = context, },

            _ => throw new ArgumentOutOfRangeException(nameof(viewModel)),
        };
    }
}