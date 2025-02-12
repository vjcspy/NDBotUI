using System;
using System.ComponentModel;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.MementoMori.Store;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.EventManager;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainerViewModel : ObservableViewModelBase
{
    public AppStore Store { get; } = AppStore.Instance;

    [ObservableProperty] public string toggleButtonText = "Please select emulator";

    [RelayCommand]
    public void ToggleReRollCommand()
    {
        RxEventManager.Dispatch(MoriAction.StartMoriReRoll.Create());
    }

    public MoriConfigContainerViewModel()
    {
        Console.WriteLine($"MoriConfigContainerViewModel");
        AppStore.Instance.EmulatorStore.ObservableForProperty(state => state.State)
            .AutoDispose(newVale =>
            {
                if (newVale.Value.SelectedEmulatorId is not null)
                {
                    ToggleButtonText = "Start";
                }
            }, Disposables);
    }

    // Phương thức xử lý khi Id thay đổi
    private void OnEmulatorConnectionIdChanging(object sender, PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(EmulatorConnection.Id))
        {
            // Thực hiện hành động khi Id thay đổi
            Console.WriteLine("EmulatorConnection.Id is changing!");
        }
    }
}