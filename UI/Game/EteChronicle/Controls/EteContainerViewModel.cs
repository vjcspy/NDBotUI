using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public class EteContainerViewModel: ViewModelBase, IRoutableViewModel
{
    private bool _isLoading = true;

    public EteContainerViewModel(IScreen screen)
    {
        HostScreen = screen;
        AppStore
            .Instance
            .EmulatorStore
            .ObservableForProperty(state => state.State)
            .AutoDispose(
                newVale =>
                {
                    if (newVale.Value.IsLoaded)
                    {
                        IsLoading = false;
                    }
                },
                Disposables
            );
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}