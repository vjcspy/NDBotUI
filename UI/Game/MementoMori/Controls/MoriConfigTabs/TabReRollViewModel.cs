using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
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
    [ObservableProperty] public string toggleButtonText = "Please select an emulator";

    public TabReRollViewModel()
    {
        AppStore.Instance.EmulatorStore.State.ObservableForProperty(state => state.SelectedEmulatorId)
            .CombineLatest(
                AppStore.Instance.MoriStore.State.ObservableForProperty(state => state),
                (selectedEmulatorId, moriState) => new { moriState, selectedEmulatorId })
            .AutoDispose(combined  =>
            {
                Logger.Info("TabReRollViewModel data changed");
                var moriState = combined.moriState;
                var selectedEmulatorId = combined.selectedEmulatorId.Value;

                if (selectedEmulatorId is null)
                {
                    return;
                }

                var gameInstance = moriState.Value.GetGameInstance(selectedEmulatorId);

                if (gameInstance != null)
                {
                    if (gameInstance.JobType == MoriJobType.None || gameInstance.JobType == MoriJobType.ReRoll)
                    {
                        if (gameInstance.State == AutoState.On)
                            ToggleButtonText = "Stop";
                        else
                            ToggleButtonText = "Start";
                    }
                    else
                    {
                        ToggleButtonText = "Đang thực hiện Job Khác";
                    }
                }
                ToggleButtonText = "Wait";
                
            }, Disposables);
    }

    [RelayCommand]
    public void ToggleReRollCommand()
    {
        if (ToggleButtonText is "Start" or "Stop")
            if (AppStore.Instance.EmulatorStore.State.SelectedEmulatorId is { } selectedEmulatorId)
                RxEventManager.Dispatch(MoriAction.ToggleStartStopMoriReRoll.Create(
                    new BaseActionPayload(selectedEmulatorId)));

        // RxEventManager.Dispatch(MoriAction.TriggerManually.Create());
    }
}