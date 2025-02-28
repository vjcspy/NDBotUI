using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999GameInstanceViewModel: ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [RelayCommand]
    public void ReloadEmulator()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorRefresh.Create());
    }
}