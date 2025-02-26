using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.R1999.Db;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.R1999.Controls;

public partial class R1999ConfigViewModel : ObservableViewModelBase
{
    [ObservableProperty] public bool isShowConfig = false;

    public R1999ConfigViewModel()
    {
        AppStore
            .Instance
            .EmulatorStore
            .ObservableForProperty(state => state.State)
            .AutoDispose(
                newVale =>
                {
                    if (newVale.Value.SelectedEmulatorId is not null)
                    {
                        IsShowConfig = true;
                    }
                },
                Disposables
            );
    }
}