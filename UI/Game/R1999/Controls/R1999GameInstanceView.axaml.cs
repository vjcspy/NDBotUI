using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999GameInstanceView : ReactiveUserControl<R1999GameInstanceViewModel>
{
    public R1999GameInstanceView()
    {
        DataContext = new R1999GameInstanceViewModel();
        InitializeComponent();
    }

    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedRows = DataGrid.SelectedItems;
        if (selectedRows.Count == 1 && selectedRows[0] is GameInstance gameInstance)
        {
            RxEventManager.Dispatch(
                EmulatorAction.SelectEmulatorConnection.Create(new BaseActionPayload(gameInstance.EmulatorId))
            );
        }
    }
}