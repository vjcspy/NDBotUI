using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.ViewModels;
using NLog;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public class GameInstanceViewModel : ObservableViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public AppStore Store { get; } = AppStore.Instance;
}