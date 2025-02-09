using System.Reactive;
using ReactiveUI;

namespace NDBotUI.ViewModels.TedBed;

public class ProductPageViewModel : ReactiveObject, IRoutableViewModel, IScreen
{
    public string UrlPathSegment => "product";
    public IScreen HostScreen { get; }

    // Router con để điều hướng giữa danh sách & chi tiết sản phẩm
    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToDetail { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToList { get; }

    public ProductPageViewModel(IScreen screen)
    {
        HostScreen = screen;

        // Mặc định hiển thị danh sách sản phẩm
        Router.Navigate.Execute(new ProductListViewModel(this));

        NavigateToDetail = ReactiveCommand.CreateFromObservable(() =>
            Router.Navigate.Execute(new ProductDetailViewModel(this)));
        NavigateToList = ReactiveCommand.CreateFromObservable(() =>
            Router.Navigate.Execute(new ProductListViewModel(this)));
    }
}