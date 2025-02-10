using System;
using System.Reactive;
using System.Reactive.Disposables;
using Microsoft.Extensions.DependencyInjection;
using NDBotUI.Modules.Core.Store;
using ReactiveUI;

namespace NDBotUI.UI.Base.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel
{
    public AppStore Store => AppStore.Instance;
    public RoutingState Router { get; } = new RoutingState();

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

    public MainWindowViewModel(Func<IScreen, AutoContainerViewModel> autoContainerFactory)
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
            Router.Navigate.Execute(autoContainerFactory(this));
            // Router.Navigate.Execute(new ProductPageViewModel(this));

            Disposable
                .Create(() => Console.WriteLine("MainWindowViewModel bị hủy!"))
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();
}