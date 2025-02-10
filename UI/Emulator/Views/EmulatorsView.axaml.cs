using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Emulator.ViewModels;
using NLog;

namespace NDBotUI.UI.Emulator.Views;

public partial class EmulatorsView : ReactiveUserControl<EmulatorsViewModel>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public EmulatorsView()
    {
        DataContext = new EmulatorsViewModel();
        InitializeComponent();
    }
    
    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Logger.Info("Select emulator connection" +
                    AppStore.Instance.EmulatorStore.State.EmulatorConnection?.Id
        );
    }
}