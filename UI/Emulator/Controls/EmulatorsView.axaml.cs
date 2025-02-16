using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.UI.Emulator.Controls;

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
        var selectedRows = DataGrid.SelectedItems;
        if (selectedRows.Count == 1 && selectedRows[0] is EmulatorConnection emulatorConnection)
        {
            RxEventManager.Dispatch(
                EmulatorAction.SelectEmulatorConnection.Create(
                    new BaseActionPayload(
                        emulatorConnection.Id,
                        emulatorConnection
                    )
                )
            );
        }
    }
}