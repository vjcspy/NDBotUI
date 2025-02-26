using Avalonia.ReactiveUI;
using NDBotUI.Modules.Game.R1999;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999ContainerView : ReactiveUserControl<R1999ContainerViewModel>
{
    public R1999ContainerView()
    {
        InitializeComponent();
        R1999Boot.Boot();
        RxEventManager.Dispatch(R1999Action.InitR1999.Create());
    }
}