using System;
using System.Reactive;
using System.Reactive.Disposables;
using NDBotUI.Modules.Core.ViewModels;
using NDBotUI.ViewModels.TedBed;
using ReactiveUI;
using GlobalState = NDBotUI.Modules.Core.Store.GlobalState;

namespace NDBotUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel
{
    public GlobalState State { get; } = GlobalState.Instance;
    public RoutingState Router { get; } = new RoutingState();

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;
    public MainWindowViewModel()
    {
        // Manage the routing state. Use the Router.Navigate.Execute
        // command to navigate to different view models. 
        //
        // Note, that the Navigate.Execute method accepts an instance 
        // of a view model, this allows you to pass parameters to 
        // your view models, or to reuse existing view models.
        //

        this.WhenActivated(disposables =>
        {
            Router.Navigate.Execute(new AutoContainerViewModel(this));

            Disposable
                .Create(() => Console.WriteLine("MainWindowViewModel bị hủy!"))
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();
}