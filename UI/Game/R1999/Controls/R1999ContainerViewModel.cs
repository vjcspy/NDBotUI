using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.R1999.Controls;

public class R1999ContainerViewModel: ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }

    public R1999ContainerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}