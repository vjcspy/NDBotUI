using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Emulator.Controls;

public class EmulatorsViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;
}