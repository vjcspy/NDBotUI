using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Emulator.Controls;

public class EmulatorsViewModel : ObservableViewModelBase
{
    public EmulatorsViewModel()
    {
        RxEventManager.Dispatch(EmulatorAction.EmulatorInitAction.Create());
    }

    public AppStore Store { get; } = AppStore.Instance;
}