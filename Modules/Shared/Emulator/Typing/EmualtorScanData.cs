using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;

namespace NDBotUI.Modules.Shared.Emulator.Typing;

public class EmulatorScanData(string address, AdbClient adbClient, DeviceData deviceData)
{
    public string Address { get; } = address;
    public AdbClient AdbClient { get; } = adbClient;
    public DeviceData DeviceData { get; } = deviceData;
}