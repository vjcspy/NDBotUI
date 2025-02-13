using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;
using SkiaSharp;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public record DetectedTemplatePoint(MoriTemplateKey MoriTemplateKey, Point Point);

public class DetectCurrentScreen : EffectBase
{
    private async Task<DetectedTemplatePoint?> DetectCurrentScreenAsync(SKBitmap screenShotSkBitmap,
        EmulatorConnection emulator,
        MoriTemplateKey moriTemplateKey)
    {
        Logger.Info("Starting detect current screen");
        if (TemplateImageDataHelper.IsLoaded &&
            TemplateImageDataHelper.TemplateImageData[MoriTemplateKey.StartStartButton].TemplateMat is
                { } templateMat)
        {
            var point = await emulator.GetPointByMatAsync(templateMat, false, screenShotSkBitmap);
            if (point is { } bpoint)
            {
                Logger.Info($"Found template point for key {moriTemplateKey}");

                return new DetectedTemplatePoint(MoriTemplateKey: moriTemplateKey, Point: bpoint);
            }

            Logger.Info($"Not template point for {moriTemplateKey}");
        }
        else
        {
            Logger.Info($"TemplateImageDataHelper not loaded");
        }

        return null;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return
        [
            MoriAction.EligibilityCheck
        ];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;

        Logger.Info($"Detect current screen");

        MoriTemplateKey[] screenToCheck =
        [
            MoriTemplateKey.StartStartButton,
            MoriTemplateKey.StartSettingButton
        ];

        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection == null) return CoreAction.Empty;

        // Optimize by use one screen shot
        var screenShotSkBitmap = await emulatorConnection.TakeScreenshotAsync();

        if (screenShotSkBitmap is null) return CoreAction.Empty;

        var tasks = screenToCheck.Select(moriTemplateKey =>
            Observable.FromAsync(
                    () => Task.Run(() =>
                        DetectCurrentScreenAsync(screenShotSkBitmap, emulatorConnection, moriTemplateKey))
                )
                .ObserveOn(Scheduler.Default)
                .SubscribeOn(Scheduler.Default)
        );

        var result = await tasks
            .Merge()
            .Where(res => res != null)
            .Take(1)
            .FirstAsync();

        if (result is { } detectedTemplatePoint)
        {
            Logger.Info($"Detected template point: {detectedTemplatePoint.Point}");
        }

        return CoreAction.Empty;
    }
}