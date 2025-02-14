using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using NDBotUI.Modules.Shared.Emulator.Errors;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Helpers;

public class AdbHelper(string adbPath)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private AdbServer? _adbServer;

    public void InitAdbServer(bool forceRestart = true)
    {
        if (AdbServer.Instance.GetStatus().IsRunning && forceRestart)
            try
            {
                AdbServer.Instance.StopServer();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to stop adb server");
            }

        _adbServer = new AdbServer();

        var result = _adbServer.StartServer(adbPath, false);

        if (result != StartServerResult.Started) throw new CouldNotInitAdbServer();

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

        return connectedDevices;
    }

    public AdbServer GetAdbServer()
    {
        return _adbServer ?? throw new InvalidOperationException("ADB Server is not initialized.");
    }
}