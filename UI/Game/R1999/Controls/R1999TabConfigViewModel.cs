using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.R1999.Db;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Game.R1999.Typing;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999TabConfigViewModel : ObservableViewModelBase
{
    [ObservableProperty] public string toggleButtonText = "Please select an emulator";

    public R1999TabConfigViewModel()
    {
        AppStore
            .Instance
            .EmulatorStore
            .ObservableForProperty(state => state.State.SelectedEmulatorId)
            .AutoDispose(
                selectedEmulatorIdValue =>
                {
                    var selectedEmulatorId = selectedEmulatorIdValue.Value;

                    if (selectedEmulatorId is null)
                    {
                        return;
                    }

                    var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(selectedEmulatorId);

                    if (gameInstance != null)
                    {
                        if (gameInstance.JobType == R1999JobType.None || gameInstance.JobType == R1999JobType.ReRoll)
                        {
                            if (gameInstance.State == AutoState.On)
                            {
                                ToggleButtonText = "Stop";
                            }
                            else
                            {
                                ToggleButtonText = "Start";
                            }
                        }
                        else
                        {
                            ToggleButtonText = "Đang thực hiện Job Khác";
                        }
                    }
                    else
                    {
                        ToggleButtonText = "Wait";
                    }
                },
                Disposables
            );

        AppStore
            .Instance
            .R1999Store
            .ObservableForProperty(state => state.State)
            .AutoDispose(
                state =>
                {
                    if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
                    {
                        var gameInstance =
                            state.Value.GetGameInstance(selectedEmulatorId);
                        if (gameInstance != null)
                        {
                            if (gameInstance.JobType == R1999JobType.None
                                || gameInstance.JobType == R1999JobType.ReRoll)
                            {
                                if (gameInstance.State == AutoState.On)
                                {
                                    ToggleButtonText = "Stop";
                                }
                                else
                                {
                                    ToggleButtonText = "Start";
                                }
                            }
                            else
                            {
                                ToggleButtonText = "Đang thực hiện Job Khác";
                            }
                        }
                        else
                        {
                            ToggleButtonText = "Wait";
                        }
                    }
                },
                Disposables
            );
    }

    public AppStore Store { get; } = AppStore.Instance;

    [RelayCommand]
    public void ToggleReRollCommand()
    {
        if (ToggleButtonText is "Start" or "Stop")
        {
            if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
            {
                RxEventManager.Dispatch(
                    R1999Action.ToggleStartStopReRoll.Create(
                        new BaseActionPayload(selectedEmulatorId)
                    )
                );
            }
        }

        // RxEventManager.Dispatch(MoriAction.TriggerManually.Create());
    }

    [RelayCommand]
    public void TestCommand()
    {
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