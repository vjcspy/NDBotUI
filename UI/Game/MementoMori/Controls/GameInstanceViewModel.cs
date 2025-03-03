using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class GameInstanceViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [RelayCommand]
    public void ReloadEmulator()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorRefresh.Create());
    }
}