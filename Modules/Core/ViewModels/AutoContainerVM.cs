using NDBotUI.ViewModels;
using ReactiveUI;

namespace NDBotUI.Modules.Core.ViewModels;

public class AutoContainerViewModel(IScreen screen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment { get; } = "AutoContainer";
    public IScreen HostScreen { get; } = screen;
}