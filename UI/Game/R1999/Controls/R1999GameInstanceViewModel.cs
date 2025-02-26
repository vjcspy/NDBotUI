using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.R1999.Controls;

public class R1999GameInstanceViewModel: ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;
}