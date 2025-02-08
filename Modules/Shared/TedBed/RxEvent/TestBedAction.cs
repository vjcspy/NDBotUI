using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.TedBed.RxEvent;

public static class TestBedAction
{
    private const string FOO_TYPE = "FOO_TYPE";
    private const string BAR_TYPE = "BAR_TYPE";

    public static EventActionFactory<object?> FOO_ACTION = new(FOO_TYPE);

    public static EventActionFactory<object?> BAR_ACTION = new(BAR_TYPE);
}