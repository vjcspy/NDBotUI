using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Shared.Emulator.Store;
using NDBotUI.UI.Base.Extensions;
using NDBotUI.UI.Base.ViewModels;
using ReactiveUI;

namespace NDBotUI.UI.Game.MementoMori.Controls;

public class MoriContainerViewModel : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment { get; } = "MoriContainer";
    public IScreen HostScreen { get; }

    private bool _isLoading = true;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public MoriContainerViewModel(IScreen screen)
    {
        HostScreen = screen;
        AppStore.Instance.EmulatorStore.ObservableForProperty(state => state.State)
            .AutoDispose(newVale =>
            {
                if (newVale.Value.IsLoaded)
                {
                    IsLoading = false;
                }
            }, Disposables);
    }
}