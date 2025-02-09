using System;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using NDBotUI.Modules.Shared.Emulator.Errors;

namespace NDBotUI.Modules.Shared.Emulator.Helpers;

public class AdbHelper
{
    private readonly string adbPath;
    private AdbServer adbServer;

    public AdbHelper(string adbPath)
    {
        this.adbPath = adbPath;
        InitAdbServer();
    }

    private void InitAdbServer()
    {
        if (AdbServer.Instance.GetStatus().IsRunning)
        {
            return; // Server đã chạy, không cần khởi động lại
        }

        adbServer = new AdbServer();
        var result = adbServer.StartServer(adbPath, restartServerIfNewer: false);

        if (result != StartServerResult.Started)
        {
            throw new CouldNotInitAdbServer();
        }
    }

    public AdbServer GetAdbServer()
    {
        return adbServer ?? throw new InvalidOperationException("ADB Server is not initialized.");
    }
}