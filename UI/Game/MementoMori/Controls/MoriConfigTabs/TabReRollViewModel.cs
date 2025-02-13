using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls.MoriConfigTabs;

public partial class TabReRollViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [ObservableProperty] public string toggleButtonText = "Please select emulator";

    [RelayCommand]
    public void ToggleReRollCommand()
    {
        // RxEventManager.Dispatch(MoriAction.StartMoriReRoll.Create());
        RxEventManager.Dispatch(MoriAction.TriggerManually.Create());
    }

    public TabReRollViewModel()
    {
        AppStore.Instance.EmulatorStore.ObservableForProperty(state => state.State)
            .AutoDispose(newVale =>
            {
                if (newVale.Value.SelectedEmulatorId is not null)
                {
                    ToggleButtonText = "Start";
                }
            }, Disposables);
    }
}