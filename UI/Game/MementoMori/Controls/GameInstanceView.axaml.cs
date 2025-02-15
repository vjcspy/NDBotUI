using Avalonia.Controls;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class GameInstanceView : UserControl
{
    public GameInstanceView()
    {
        DataContext = new GameInstanceViewModel();
        InitializeComponent();
    }

    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedRows = DataGrid.SelectedItems;
        if (selectedRows.Count == 1 && selectedRows[0] is GameInstance gameInstance)
            RxEventManager.Dispatch(
                EmulatorAction.SelectEmulatorConnection.Create(new BaseActionPayload(gameInstance.EmulatorId)));
    }
}