using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public partial class MoriConfigContainerViewModel : ObservableViewModelBase
{
    [ObservableProperty] public bool isShowConfig = false;

    public MoriConfigContainerViewModel()
    {
        AppStore.Instance.EmulatorStore.ObservableForProperty(state => state.State)
            .AutoDispose(newVale =>
            {
                if (newVale.Value.SelectedEmulatorId is not null)
                {
                    IsShowConfig = true;
                }
            }, Disposables);
    }
}