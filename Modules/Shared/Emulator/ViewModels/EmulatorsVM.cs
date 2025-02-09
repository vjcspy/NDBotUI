using System.Collections.ObjectModel;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.ViewModels;

namespace NDBotUI.Modules.Shared.Emulator.ViewModels;

public class EmulatorsVM : ViewModelBase
{
    public ObservableCollection<EmulatorConnection> EmulatorConnections { get; } =
        new(EmulatorManager.Instance.EmulatorConnections);
}