using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NLog;

namespace NDBotUI.Modules.Shared.Emulator.Models;

public class EmulatorConnection(EmulatorScanData emulatorScanData)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public DeviceData DeviceData { get; } = emulatorScanData.DeviceData;
    private DeviceClient? _deviceClient;

    public string Id => emulatorScanData.DeviceData.Serial;
    public string Serial => emulatorScanData.DeviceData.Serial;
    public DeviceState State => emulatorScanData.DeviceData.State;
    public string DeviceType => DetectEmulatorType(emulatorScanData.DeviceData.Model);

    public string SendShellCommand(string command)
    {
        var receiver = new ConsoleOutputReceiver();
        emulatorScanData.AdbClient.ExecuteRemoteCommand(command, emulatorScanData.DeviceData, receiver);

        return receiver.ToString();
    }

    public DeviceClient GetDeviceClient()
    {
        if (_deviceClient == null)
        {
            _deviceClient = new DeviceClient(emulatorScanData.AdbClient, emulatorScanData.DeviceData);
        }

        return _deviceClient;
    }

    private string DetectEmulatorType(string model)
    {
        if (model.Contains(EmulatorTypes.Bluestacks, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.Bluestacks;
        if (model.Contains(EmulatorTypes.Nox, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.Nox;
        if (model.Contains(EmulatorTypes.LDPlayer, StringComparison.OrdinalIgnoreCase))
            return EmulatorTypes.LDPlayer;

        return "Unknown";
    }

    public async Task<Point?> getPointByImage(object image)
    {
        var framebuffer = await emulatorScanData.AdbClient.GetFrameBufferAsync(emulatorScanData.DeviceData);

        try
        {
            var bitmap = framebuffer.ToSKBitmap();

            // Thư mục lưu ảnh
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Định dạng tên file theo thời gian hiện tại: yyyyMMdd_HHmmss.jpg
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string jpgPath = Path.Combine(folderPath, $"screenshot_{timestamp}.jpg");

            // Lưu ảnh dưới dạng JPG
            bitmap?.SaveAsJpeg(jpgPath);
            Logger.Info($"Screenshot saved to {jpgPath}");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to load screenshot from {image}. Error: " + e.Message);
        }


        return null;
    }
}