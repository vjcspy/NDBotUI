using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public record EmulatorState(
    Lst<EmulatorConnection> EmulatorConnections,
    bool IsLoaded,
    string? SelectedEmulatorId,
    int Attempts = 0)
{
    public static EmulatorState factory()
    {
        return new EmulatorState([], false, null);
    }
}