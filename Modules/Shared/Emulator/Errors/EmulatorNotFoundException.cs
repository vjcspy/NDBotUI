using System;

namespace NDBotUI.Modules.Shared.Emulator.Errors;

public class EmulatorNotFoundException : Exception
{
    public EmulatorNotFoundException() : base($"Emulator not found.")
    {
    }

    public EmulatorNotFoundException(string message) : base(message)
    {
    }

    public EmulatorNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}