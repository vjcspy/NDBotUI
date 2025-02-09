using System.Reactive;
using NDBotUI.Store;
using NDBotUI.ViewModels.TedBed;
using ReactiveUI;

namespace NDBotUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public GlobalState State { get; } = GlobalState.Instance; 

    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; } = new RoutingState();

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToProductPage { get; }

    public MainWindowViewModel()
    {
        // Manage the routing state. Use the Router.Navigate.Execute
        // command to navigate to different view models. 
        //
        // Note, that the Navigate.Execute method accepts an instance 
        // of a view model, this allows you to pass parameters to 
        // your view models, or to reuse existing view models.
        //
        NavigateToProductPage = ReactiveCommand.CreateFromObservable(() =>
            Router.Navigate.Execute(new ProductPageViewModel(this)));
    }
}