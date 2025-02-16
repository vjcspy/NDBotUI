using System.Reactive;
using ReactiveUI;

namespace NDBotUI.UI.TedBed.ViewModels;

public class ProductListViewModel : ReactiveObject, IRoutableViewModel, IScreen
{
    public ProductListViewModel(IScreen screen)
    {
        HostScreen = screen;
        NavigateToDetail = ReactiveCommand.CreateFromObservable(
            () =>
                Router.Navigate.Execute(new ProductDetailViewModel(this))
        );
    }

    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToDetail { get; }

    public string UrlPathSegment
    {
        get => "list";
    }

    public IScreen HostScreen { get; }
    public RoutingState Router { get; } = new();
}