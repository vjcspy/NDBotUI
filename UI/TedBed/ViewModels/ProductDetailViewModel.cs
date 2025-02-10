using System.Reactive;
using ReactiveUI;

namespace NDBotUI.UI.TedBed.ViewModels;

public class ProductDetailViewModel : ReactiveObject, IRoutableViewModel, IScreen
{
    public string UrlPathSegment => "detail";
    public IScreen HostScreen { get; }
    public RoutingState Router { get; } = new();
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateBack { get; }

    public ProductDetailViewModel(IScreen screen)
    {
        HostScreen = screen;

        NavigateBack = ReactiveCommand.CreateFromObservable(() =>
            HostScreen.Router.NavigateBack.Execute(Unit.Default));
    }
}