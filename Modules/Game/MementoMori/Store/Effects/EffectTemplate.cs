using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.MementoMori.Helper;
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
        if (EmulatorManager.Instance.EmulatorConnections.Count != 1) return CoreAction.Empty;

        var emulator = EmulatorManager.Instance.EmulatorConnections[0];

        /* Test take resolution */
        // var resolution = emulator.GetScreenResolution();
        // Logger.Debug($"Screen resolution: {resolution}");

        /* Test find template */
        if (TemplateImageDataHelper.IsLoaded &&
            TemplateImageDataHelper.TemplateImageData[MoriTemplateKey.StartStartButton].EmuCVMat is
                { } templateMat)
        {
            var screenshot = await emulator.TakeScreenshotAsync();
            if (screenshot is null) return CoreAction.Empty;

            var screenshotMat = screenshot.ToEmguMat();
            // ImageFinderEmguCV.SaveMatToFile(screenshotMat, "screenshot1.png");
            // ImageFinderEmguCV.SaveMatToFile(templateMat, "template1.png");
            var point = ImageFinderEmguCV.FindTemplateMatPoint(screenshotMat, templateMat);
            if (point != null)
                Logger.Info($"Point is {point.Value.X}, {point.Value.Y}");
            else
                Logger.Info("Point is null");
        }
        else
        {
            Logger.Info("TemplateImageDataHelper not loaded");
        }


        return CoreAction.Empty;
    }

    [Effect]
    public RxEventHandler EffectHandler()
    {
        return upstream => upstream.OfAction(MoriAction.TriggerManually).SelectMany(Process);
    }
}