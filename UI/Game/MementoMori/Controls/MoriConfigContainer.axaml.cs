using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainer : ReactiveUserControl<MoriConfigContainerViewModel>
{
    public MoriConfigContainer()
    {
        DataContext = new MoriConfigContainerViewModel();
        InitializeComponent();
    }
}