using System;
using System.Reactive.Disposables;

namespace NDBotUI.UI.Base.Extensions;

public static class DisposableExtensions
{
    /// <summary>
    ///     Tự động subscribe vào một observable và thêm vào CompositeDisposable
    /// </summary>
    public static void AutoDispose<T>(
        this IObservable<T> observable,
        Action<T> onNext,
        CompositeDisposable disposables)
    {
        disposables.Add(observable.Subscribe(onNext));
    }
}