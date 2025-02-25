using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.R1999.Controls;

public class R1999ContainerViewModel : ViewModelBase, IRoutableViewModel
{
    private bool _isLoading = true;

    public R1999ContainerViewModel(IScreen screen)
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