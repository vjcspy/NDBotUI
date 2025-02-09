using System;
using System.Linq;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using NDBotUI.Modules.Shared.Emulator.Errors;

namespace NDBotUI.Modules.Shared.Emulator.Models;

public class EmulatorConnection
{
    public string Id { get; } // Serial
    public string EmulatorType { get; }
    public bool IsConnected { get; private set; }

    private readonly AdbClient adbClient;
    private DeviceData deviceData;

    public EmulatorConnection(string id, string emulatorType, AdbClient adbClient)
    {
        Id = id;
        EmulatorType = emulatorType;
        this.adbClient = adbClient;
    }

    public void Connect()
    {
        try
        {
            deviceData = adbClient.GetDevices().First(d => d.Serial == Id);

            if (deviceData == null)
            {
                throw new EmulatorNotFoundException($"Device with ID {Id} not found.");
            }
            
            IsConnected = true;
            Console.WriteLine($"Connected to emulator {Id} ({EmulatorType}).");
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void Disconnect()
    {
        IsConnected = false;
        Console.WriteLine($"Disconnected from emulator {Id}.");
    }

    public string SendShellCommand(string command)
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to the emulator.");

        var receiver = new ConsoleOutputReceiver();
        adbClient.ExecuteRemoteCommand(command, deviceData, receiver);

        return receiver.ToString();
    }

    public DeviceClient GetDeviceClient()
    {
        if (!IsConnected) throw new InvalidOperationException("Not connected to the emulator.");

        return new DeviceClient(adbClient, deviceData);
    }
}