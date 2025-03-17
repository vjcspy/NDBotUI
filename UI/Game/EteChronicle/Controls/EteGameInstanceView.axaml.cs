using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.Ete.Store;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteGameInstanceView : UserControl
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public EteGameInstanceView()
    {
        DataContext = new EteGameInstanceViewModel();
        InitializeComponent();
    }

    public void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Logger.Info("Selection Emulator changed");
        var selectedRows = DataGrid.SelectedItems;
        if (selectedRows.Count == 1 && selectedRows[0] is EteGameInstance gameInstance)
        {
            RxEventManager.Dispatch(
                EmulatorAction.SelectEmulatorConnection.Create(new BaseActionPayload(gameInstance.EmulatorId))
            );
        }
    }
}