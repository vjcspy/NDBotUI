using System;
using System.Reactive.Disposables;
using NLog;
using ReactiveUI;

namespace NDBotUI.UI.Base.ViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    protected readonly CompositeDisposable Disposables = new();

    public void Dispose()
    {
        Disposables.Dispose();
        Logger.Info("Disposed ViewModel");
    }
}