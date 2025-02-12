using System;
using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.ComponentModel;
using NLog;

namespace NDBotUI.UI.Base.ViewModels;

public class ObservableViewModelBase : ObservableObject, IDisposable
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected readonly CompositeDisposable Disposables = new();

    public void Dispose()
    {
        Disposables.Dispose();
        Logger.Info("Disposed ViewModel");
    }
}