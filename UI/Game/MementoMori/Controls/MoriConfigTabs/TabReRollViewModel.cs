using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Typing;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls.MoriConfigTabs;

public partial class TabReRollViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [ObservableProperty] public string toggleButtonText = "Please select an emulator";

    [RelayCommand]
    public void ToggleReRollCommand()
    {
        if (ToggleButtonText is "Start" or "Stop")
        {
            RxEventManager.Dispatch(MoriAction.ToggleStartStopMoriReRoll.Create());
        }

        // RxEventManager.Dispatch(MoriAction.TriggerManually.Create());
    }

    public TabReRollViewModel()
    {
        AppStore.Instance.MoriStore.ObservableForProperty(state => state.State)
            .AutoDispose(moriState =>
            {
                var gameInstance = moriState.Value.GetCurrentEmulatorGameInstance();

                if (gameInstance != null)
                {
                    if (gameInstance.JobType == MoriJobType.None || gameInstance.JobType == MoriJobType.ReRoll)
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
            }, Disposables);
    }
}