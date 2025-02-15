using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public class GameInstanceViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;
}