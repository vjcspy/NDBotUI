using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NLog;
using Point = System.Drawing.Point;

namespace NDBotUI.Modules.Shared.Emulator.Models;

public class EmulatorConnection(EmulatorScanData emulatorScanData)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private int[]? _cacheScreenResolution;
    public DeviceData DeviceData { get; } = emulatorScanData.DeviceData;

    public string Id
    {
        get => emulatorScanData.DeviceData.Serial;
    }

    public string Serial
    {
        get => emulatorScanData.DeviceData.Serial;
    }

    public DeviceState State
    {
        get => emulatorScanData.DeviceData.State;
    }

    public string DeviceType
    {
        get => DetectEmulatorType(emulatorScanData.DeviceData.Model);
    }

    public string SendShellCommand(string command)
    {
        var receiver = new ConsoleOutputReceiver();
        emulatorScanData.AdbClient.ExecuteRemoteCommand(command, emulatorScanData.DeviceData, receiver);

        return receiver.ToString();
    }

    private string DetectEmulatorType(string model)
    {
        if (model.Contains(EmulatorTypes.Bluestacks, StringComparison.OrdinalIgnoreCase))
        {
            return EmulatorTypes.Bluestacks;
        }

        if (model.Contains(EmulatorTypes.Nox, StringComparison.OrdinalIgnoreCase))
        {
            return EmulatorTypes.Nox;
        }

        if (model.Contains(EmulatorTypes.LDPlayer, StringComparison.OrdinalIgnoreCase))
        {
            return EmulatorTypes.LDPlayer;
        }

        return "Unknown";
    }

    public async Task<Framebuffer?> TakeScreenshotAsync()
    {
        Logger.Debug("TakeScreenshotAsync");
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var screenshot = await emulatorScanData.AdbClient.GetFrameBufferAsync(emulatorScanData.DeviceData);
            stopwatch.Stop();
            Logger.Info("TakeScreenshotAsync finished in {time} ms", stopwatch.ElapsedMilliseconds);
            return screenshot;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Emulator {Id} failed to TakeScreenshotAsync");
            return null;
        }
    }

    // public async Task<SKBitmap?> TakeScreenshotSKBitmapAsync(bool isSaveToDir = false)
    // {
    //     Logger.Info($"TakeScreenshotSKBitmapAsync {isSaveToDir}");
    //     var stopwatch = Stopwatch.StartNew();
    //     var framebuffer = await TakeScreenshotAsync();
    //
    //     if (framebuffer == null) return null;
    //
    //     try
    //     {
    //         var bitmap = framebuffer.ToSKBitmap();
    //         stopwatch.Stop();
    //         Logger.Info("TakeScreenshotSKBitmapAsync finished in {time} ms", stopwatch.ElapsedMilliseconds);
    //
    //         if (!isSaveToDir) return bitmap;
    //
    //         // Thư mục lưu ảnh
    //         var folderPath = FileHelper.CreateFolderIfNotExist(CoreValue.ScreenShotFolder);
    //
    //         // Định dạng tên file theo thời gian hiện tại: yyyyMMdd_HHmmss.jpg
    //         var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //         var jpgPath = Path.Combine(folderPath, $"screenshot_{timestamp}.jpg");
    //
    //         bitmap?.SaveAsPng(jpgPath);
    //         Logger.Info($"Screenshot saved to {jpgPath}");
    //
    //         return bitmap;
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.Error(ex, $"Emulator {Id} failed to TakeScreenshotAsync");
    //     }
    //
    //     return null;
    // }

    public async Task<Unit> ClickOnPointAsync(Point point)
    {
        Logger.Info($"Emulator: {Id} - Click on: {point}");
        await emulatorScanData.AdbClient.ClickAsync(emulatorScanData.DeviceData, point);

        return Unit.Default;
    }

    public async Task<Unit> SwipeAsync(Point from, Point to, int speed = 1000)
    {
        Logger.Info($"Emulator: {Id} - Swipe from: {from} to: {to}");
        await emulatorScanData.AdbClient.SwipeAsync(emulatorScanData.DeviceData, from, to, speed);

        return Unit.Default;
    }

    public async Task<Unit> SwipePPointAsync(PPoint from, PPoint to, int speed = 1000)
    {
        var currentResolution = GetScreenResolution();

        if (currentResolution == null)
        {
            Logger.Error($"Emulator: {Id} - SwipePPointAsync could not find resolution");
            return Unit.Default;
        }

        var xi = Convert.ToInt32(from.X * currentResolution[0] / 100);
        var yi = Convert.ToInt32(from.Y * currentResolution[1] / 100);
        var xf = Convert.ToInt32(to.X * currentResolution[0] / 100);
        var yf = Convert.ToInt32(to.Y * currentResolution[1] / 100);

        return await SwipeAsync(new Point(xi, yi), new Point(xf, yf), speed);
    }

    public Unit ClickOnPoint(Point point)
    {
        Logger.Info($"Emulator: {Id} - Click on: {point}");
        emulatorScanData.AdbClient.Click(emulatorScanData.DeviceData, point);

        return Unit.Default;
    }

    public string? ExecuteRemoteCommand(string command)
    {
        Logger.Info($"Emulator: {Id} - ExecuteRemoteCommand: {command}");
        emulatorScanData.AdbClient.ExecuteRemoteCommand(command, emulatorScanData.DeviceData);
        IShellOutputReceiver receiver = new ConsoleOutputReceiver();
        emulatorScanData.AdbClient.ExecuteRemoteCommand("wm size", emulatorScanData.DeviceData, receiver);
        var allOutput = receiver.ToString();
        Logger.Info($"Emulator: {Id} - ExecuteRemoteCommand Output: {allOutput}");
        return allOutput;
    }

    public int[]? GetScreenResolution()
    {
        if (_cacheScreenResolution != null)
        {
            return _cacheScreenResolution;
        }

        var resolutionText = ExecuteRemoteCommand("wm size");
        // Kiểm tra kết quả đầu ra
        if (!string.IsNullOrEmpty(resolutionText) && resolutionText.Contains("Physical size:"))
        {
            var resolution = resolutionText
                .Split(':')[1]
                .Trim(); // Lấy phần "1080x1920"
            var parts = resolution.Split('x'); // Tách thành ["1080", "1920"]

            if (parts.Length == 2 && int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height))
            {
                _cacheScreenResolution = [width, height,];

                return _cacheScreenResolution;
            }
        }

        return null; // Trả về null nếu không lấy được độ phân giải
    }

    public Unit ClickPPoint(PPoint pPoint)
    {
        var currentResolution = GetScreenResolution();

        if (currentResolution == null)
        {
            Logger.Error($"Emulator: {Id} - ClickPercent could not find resolution");
            return Unit.Default;
        }

        var xi = Convert.ToInt32(pPoint.X * currentResolution[0] / 100);
        var yi = Convert.ToInt32(pPoint.Y * currentResolution[1] / 100);

        return ClickOnPoint(new Point(xi, yi));
    }

    public async Task<Unit> ClickPPointAsync(PPoint pPoint)
    {
        var currentResolution = GetScreenResolution();

        if (currentResolution == null)
        {
            Logger.Error($"Emulator: {Id} - ClickPercentAsync could not find resolution");
            return Unit.Default;
        }

        var xi = Convert.ToInt32(pPoint.X * currentResolution[0] / 100);
        var yi = Convert.ToInt32(pPoint.Y * currentResolution[1] / 100);

        await ClickOnPointAsync(new Point(xi, yi));

        return Unit.Default;
    }
    public PPoint? ToPPoint(Point point)
    {
        var currentResolution = GetScreenResolution();

        if (currentResolution == null)
        {
            Logger.Error($"Emulator: {Id} - ToPPoint could not find resolution");
            return null;
        }

        return new PPoint(point.X * 100 / currentResolution[0], point.Y * 100 / currentResolution[1]);
    }

    public void SendText(string text)
    {
        emulatorScanData.AdbClient.SendText(emulatorScanData.DeviceData, text);
    }

    public void ClearInput()
    {
        emulatorScanData.AdbClient.ClearInput(emulatorScanData.DeviceData, 25);
    }

    public async Task SendTextAsync(string text)
    {
        await emulatorScanData.AdbClient.SendTextAsync(emulatorScanData.DeviceData, text);
    }
}