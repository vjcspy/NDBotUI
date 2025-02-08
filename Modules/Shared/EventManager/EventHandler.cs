using System;

namespace NDBotUI.Modules.Shared.EventManager;

public delegate IObservable<EventAction<object?>> RxEventHandler(IObservable<EventAction<object?>> upstream);
