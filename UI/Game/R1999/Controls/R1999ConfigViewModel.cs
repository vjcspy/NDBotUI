using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.R1999.Db;
using NDBotUI.UI.Base.ViewModels;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999ConfigViewModel : ObservableViewModelBase
{
    [RelayCommand]
    public void TestCommand()
    {
        ScreenHelper.TakeScreenshot("Reverse: 1999", "1.png");
        TestDb();
    }

    private async Task<Unit> TestEmail()
    {
        GmailAPIHelper gmailAPIHelper = new();
        var listemail = await gmailAPIHelper.GetEmailListAsync();
        if (listemail.Count > 0)
        {
            var message = await gmailAPIHelper.GetEmailByIdAsync(listemail[0].Id);
            Logger.Info($"Content message: {message.Body}");
        }


        return Unit.Default;
    }

    private void TestDb()
    {
        using (var context = new ApplicationDbContext())
        {
            var newAccount = new R1999Account
            {
                Email = "test@example.com",
                Ordinal = 1,
                AccountStatus = AccountStatus.Open,
            };

            context.R1999Accounts.Add(newAccount);
            context.SaveChanges();

            // Kiểm tra các giá trị CreatedAt và UpdatedAt
            Console.WriteLine($"CreatedAt: {newAccount.CreatedAt}, UpdatedAt: {newAccount.UpdatedAt}");
        }
    }
}