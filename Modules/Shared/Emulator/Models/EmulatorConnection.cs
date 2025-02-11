using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using Emgu.CV;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Values;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NLog;
using NLog.Fluent;
using SkiaSharp;

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

    public async Task<SKBitmap?> TakeScreenshotAsync(bool isSaveToDir = false)
    {
        var framebuffer = await emulatorScanData.AdbClient.GetFrameBufferAsync(emulatorScanData.DeviceData);
        try
        {
            var bitmap = framebuffer.ToSKBitmap();

            if (!isSaveToDir) return bitmap;

            // Thư mục lưu ảnh
            var folderPath = FileHelper.CreateFolderIfNotExist(CoreValue.ScreenShotFolder);

            // Định dạng tên file theo thời gian hiện tại: yyyyMMdd_HHmmss.jpg
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var jpgPath = Path.Combine(folderPath, $"screenshot_{timestamp}.jpg");

            bitmap?.SaveAsPng(jpgPath);
            Logger.Info($"Screenshot saved to {jpgPath}");

            return bitmap;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Emulator {Id} failed to TakeScreenshot");
        }

        return null;
    }

    public async Task<Point?> GetPointByMatAsync(Mat templateMat, bool isSaveMarkedImage = false)
    {
        var screenshot = await TakeScreenshotAsync();

        if (screenshot == null) return null;

        string? markedScreenshotPath = null;

        if (isSaveMarkedImage)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), CoreValue.ScreenShotFolder);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            markedScreenshotPath = Path.Combine(folderPath, $"marked_screenshot_{timestamp}.png");
        }

        // 🔍 Tìm kiếm ảnh trong screenshot
        var matchPoint =
            ImageProcessingHelper.FindImageInScreenshot(screenshot, templateMat, markedScreenshotPath);

        if (matchPoint.HasValue)
        {
            Logger.Info($"OpenCV found template image at {matchPoint.Value}");

            return matchPoint;
        }

        Logger.Info("OpenCV could not found template image");

        return null;
    }

    public async Task<Unit> clickOnPointAsync(Point point)
    {
        Logger.Info($"Emulator: {Id} - Click on: {point}");
        await emulatorScanData.AdbClient.ClickAsync(emulatorScanData.DeviceData, point);

        return Unit.Default;
    }
}