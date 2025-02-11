using Avalonia.Controls;
using Avalonia.Interactivity;
using NDBotUI.Modules.Shared.Emulator.Services;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainer : UserControl
{
    public MoriConfigContainer()
    {
        InitializeComponent();
    }

    private void RollButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (EmulatorManager.Instance.EmulatorConnections.Count == 1)
        {
            var emulator = EmulatorManager.Instance.EmulatorConnections[0];
            emulator.getPointByImage(null);
        }
    }
}