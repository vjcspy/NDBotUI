using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Helper;
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
        Test();
    }

    private async Task<Unit> Test()
    {
        GmailAPIHelper gmailAPIHelper = new();
        var listemail = await gmailAPIHelper.GetEmailListAsync();
        if (listemail.Count > 0)
        {
            var message =await gmailAPIHelper.GetEmailByIdAsync(listemail[0].Id);
            Logger.Info($"Content message: {message.Body}");
        }


        return Unit.Default;
    }
}