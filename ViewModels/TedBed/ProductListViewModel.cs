using System.Reactive;
using ReactiveUI;

namespace NDBotUI.ViewModels.TedBed;

public class ProductListViewModel : ReactiveObject, IRoutableViewModel, IScreen
{
    public string UrlPathSegment => "list";
    public IScreen HostScreen { get; }
    public RoutingState Router { get; } = new();
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToDetail { get; }

    public ProductListViewModel(IScreen screen)
    {
        HostScreen = screen;
        NavigateToDetail = ReactiveCommand.CreateFromObservable(() =>
            Router.Navigate.Execute(new ProductDetailViewModel(this)));
    }
}