using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class EffectTemplate
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task<EventAction> Process(EventAction action)
    {
        await Task.Delay(0);
        Logger.Info("Processing Trigger Manually Effect");
        if (EmulatorManager.Instance.EmulatorConnections.Count != 1)
        {
            return CoreAction.Empty;
        }

        var emulatorConnection = EmulatorManager.Instance.EmulatorConnections[0];
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        var stopWatch = Stopwatch.StartNew();
        var openCVMat = SkiaHelper.SkiaBitmapToMat(screenshot.ToSKBitmap());
        stopWatch.Stop();
        Logger.Info($"Convert SKBitmap to Mat took {stopWatch.ElapsedMilliseconds} ms");
        CvInvoke.Imwrite("1.png", openCVMat); // Lưu ảnh xuống file PNG

        /* Test take resolution */
        // var resolution = emulator.GetScreenResolution();
        // Logger.Debug($"Screen resolution: {resolution}");

        /* Test find template */
        // if (TemplateImageDataHelper.IsLoaded &&
        //     TemplateImageDataHelper.TemplateImageData[MoriTemplateKey.StartStartButton].EmuCVMat is
        //         { } templateMat)
        // {
        //     var screenshot = await emulator.TakeScreenshotAsync();
        //     if (screenshot is null) return CoreAction.Empty;
        //
        //     var stopwatch = Stopwatch.StartNew();
        //     var screenshotMat = screenshot.ToEmguMat();
        //     stopwatch.Stop();
        //     Logger.Info($"Screenshot to Emgu Mat took {stopwatch.ElapsedMilliseconds} ms");
        //     // ImageFinderEmguCV.SaveMatToFile(screenshotMat, "screenshot1.png");
        //     // ImageFinderEmguCV.SaveMatToFile(templateMat, "template1.png");
        //     // var point = ImageFinderEmguCV.FindTemplateORB(screenshotMat, templateMat);
        //     var point = ImageFinderEmguCV.FindTemplateMatPoint(screenshotMat, templateMat, "screenshot_marked.png");
        //     if (point != null)
        //         Logger.Info($"Point is {point.Value.X}, {point.Value.Y}");
        //     else
        //         Logger.Info("Point is null");
        // }
        // else
        // {
        //     Logger.Info("TemplateImageDataHelper not loaded");
        // }

        /* Dùng API Windows để take screenshot nhưng bị lệch và không hoạt động khi minimized */
        // ScreenCapture.TakeScreenshot("BlueStacks App Player", ImageHelper.GetImagePath("BlueStacks-Screenshot.png"));


        return CoreAction.Empty;
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(MoriAction.TriggerManually)
            .SelectMany(Process);
    }
}