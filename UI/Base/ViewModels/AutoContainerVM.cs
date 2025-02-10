using ReactiveUI;

namespace NDBotUI.UI.Base.ViewModels;

public class AutoContainerViewModel(IScreen screen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment { get; } = "AutoContainer";
    public IScreen HostScreen { get; } = screen;
}