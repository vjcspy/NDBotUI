using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public class MoriContainerViewModel(IScreen screen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment { get; } = "MoriContainer";
    public IScreen HostScreen { get; } = screen;
}