using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteGameInstanceViewModel:ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [RelayCommand]
    public void ReloadEmulator()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorRefresh.Create());
    }
}