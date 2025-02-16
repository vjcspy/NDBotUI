using LanguageExt;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Shared.Emulator.Store;

public record EmulatorState(
    Lst<EmulatorConnection> EmulatorConnections,
    bool IsLoaded,
    string? SelectedEmulatorId,
    int Attempts = 0
)
{
    public static EmulatorState factory()
    {
        return new EmulatorState([], false, null);
    }

    public EmulatorConnection? GetEmulatorConnection(string id)
    {
        return EmulatorConnections
            .Find(e => e.Id == id)
            .Match(x => x, () => null!);
    }
}