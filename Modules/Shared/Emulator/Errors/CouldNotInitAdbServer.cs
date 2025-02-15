using System;

namespace NDBotUI.Modules.Shared.Emulator.Errors;

public class CouldNotInitAdbServer : Exception
{
    public CouldNotInitAdbServer() : base("Couldn't init Adb Server")
    {
    }

    public CouldNotInitAdbServer(string message) : base(message)
    {
    }

    public CouldNotInitAdbServer(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}