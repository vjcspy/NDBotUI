using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999ConfigViewModel:ObservableViewModelBase
{
    [RelayCommand]
    public void TestCommand()
    {
        ScreenHelper.TakeScreenshot("Reverse: 1999", "1.png");
    }
}