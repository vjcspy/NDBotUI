using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999GameInstanceView : ReactiveUserControl<R1999GameInstanceViewModel>
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public R1999GameInstanceView()
    {
        DataContext = new R1999GameInstanceViewModel();
        InitializeComponent();
    }

    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Logger.Info("Selection Emulator changed");
        var selectedRows = DataGrid.SelectedItems;
        if (selectedRows.Count == 1 && selectedRows[0] is R1999GameInstance gameInstance)
        {
            RxEventManager.Dispatch(
                EmulatorAction.SelectEmulatorConnection.Create(new BaseActionPayload(gameInstance.EmulatorId))
            );
        }
    }
}