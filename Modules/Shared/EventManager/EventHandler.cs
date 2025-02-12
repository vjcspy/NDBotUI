using System;

namespace NDBotUI.Modules.Shared.EventManager;

public delegate IObservable<EventAction> RxEventHandler(IObservable<EventAction> upstream);
