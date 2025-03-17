using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.Ete.Store;
using NDBotUI.Modules.Game.Ete.Typing;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Game.R1999.Typing;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.EteChronicle.Controls;

public partial class EteTabConfigViewModel:ObservableViewModelBase
{
     [ObservableProperty] public string toggleButtonText = "Please select an emulator";

    public EteTabConfigViewModel()
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

                    var gameInstance = AppStore.Instance.EteStore.State.GetGameInstance(selectedEmulatorId);

                    if (gameInstance != null)
                    {
                        if (gameInstance.JobType == EteJobType.None || gameInstance.JobType == EteJobType.ReRoll)
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
            .EteStore
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
                            if (gameInstance.JobType == EteJobType.None
                                || gameInstance.JobType == EteJobType.ReRoll)
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
                    EteAction.ToggleStartStopReRoll.Create(
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
        if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
        {
            // RxEventManager.Dispatch(
            //     R1999Action.SaveResultOk.Create(
            //         new BaseActionPayload(selectedEmulatorId)
            //     )
            // );
        }
    }
}