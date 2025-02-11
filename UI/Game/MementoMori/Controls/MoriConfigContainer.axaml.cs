using Avalonia.Controls;
using Avalonia.Interactivity;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainer : UserControl
{
    public MoriConfigContainer()
    {
        InitializeComponent();
    }

    private void RollButton_OnClick(object? sender, RoutedEventArgs e)
    {
        RxEventManager.Dispatch(MoriAction.TriggerScanCurrentScreen.Create());
    }
}