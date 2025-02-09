using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using NDBotUI.Modules.Shared.Emulator.Errors;
using NDBotUI.Modules.Shared.Emulator.Typing;

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
            Console.WriteLine("Adb server is already running.");
            return;
            // try
            // {
            //     AdbServer.Instance.StopServer();
            // }
            // catch
            // {
            //     // ignored
            // }
        }

        adbServer = new AdbServer();

        var result = adbServer.StartServer(adbPath, restartServerIfNewer: true);

        if (result != StartServerResult.Started)
        {
            throw new CouldNotInitAdbServer();
        }

        Console.WriteLine("Adb server started.");
    }

    public List<EmulatorScanData> ConnectByGetDevices()
    {
        // TODO: Vẫn chưa xử lý được việc adb không lấy được hết devices. Lần đầu bật thì chỉ ra 1 phải tắt bật lại lần 2 thì mới lấy đủ
        var adbClient = new AdbClient();
        adbClient.Connect("");
        // adbClient.Connect("127.0.0.1");
        var devices = adbClient.GetDevices();

        return devices.Select(deviceData => new EmulatorScanData(deviceData.Serial, adbClient, deviceData)).ToList();
    }

    public List<EmulatorScanData> ConnectToAllEmulators()
    {
        var connectedDevices = new List<EmulatorScanData>();

        var adbPorts = NetstatScanner.GetAdbPorts();

        foreach (var address in from port in adbPorts where port >= 5000 select $"127.0.0.1:{port}")
        {
            try
            {
                var adbClient = new AdbClient();
                adbClient.Connect(address);
                var devices = adbClient.GetDevices();
                var deviceData = devices.First();
                connectedDevices.Add(new EmulatorScanData(address, adbClient, deviceData));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to {address}: {ex.Message}");
            }
        }

        return connectedDevices;
    }

    public AdbServer GetAdbServer()
    {
        return adbServer ?? throw new InvalidOperationException("ADB Server is not initialized.");
    }
}