using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class GameInstanceView : ReactiveUserControl<GameInstanceViewModel>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public GameInstanceView()
    {
        InitializeComponent();
        DataContext = new GameInstanceViewModel();
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

    public void ConfigGameInstance(string emulatorId)
    {
        Logger.Info($"Game Instance Config: {emulatorId}");
        RxEventManager.Dispatch(
            EmulatorAction.SelectEmulatorConnection.Create(new BaseActionPayload(emulatorId))
        );
    }
}