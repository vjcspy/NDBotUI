using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.Ete;
using NDBotUI.Modules.Game.Ete.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteContainerView : ReactiveUserControl<EteContainerViewModel>
{
    public EteContainerView()
    {
        InitializeComponent();
        EteBoot.Boot();
        RxEventManager.Dispatch(EteAction.InitEte.Create());
    }
}