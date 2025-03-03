using System;
using System.Reactive;
using System.Reactive.Disposables;
using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Game.MementoMori.Controls;
using NDBotUI.UI.Game.R1999.Controls;
using ReactiveUI;

namespace NDBotUI.UI.Base.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel
{
    public MainWindowViewModel(Func<IScreen, AutoContainerViewModel> autoContainerFactory)
    {
        // Manage the routing state. Use the Router.Navigate.Execute
        // command to navigate to different view models. 
        //
        // Note, that the Navigate.Execute method accepts an instance 
        // of a view model, this allows you to pass parameters to 
        // your view models, or to reuse existing view models.
        //

        this.WhenActivated(
            disposables =>
            {
                // Router.Navigate.Execute(new R1999ContainerViewModel(this));
                Router.Navigate.Execute(new MoriContainerViewModel(this));
                // Router.Navigate.Execute(new ProductPageViewModel(this));

                Disposable
                    .Create(() => Console.WriteLine("MainWindowViewModel bị hủy!"))
                    .DisposeWith(disposables);
            }
        );
    }

    public AppStore Store
    {
        get => AppStore.Instance;
    }

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack
    {
        get => Router.NavigateBack;
    }

    public ViewModelActivator Activator { get; } = new();
    public RoutingState Router { get; } = new();
}