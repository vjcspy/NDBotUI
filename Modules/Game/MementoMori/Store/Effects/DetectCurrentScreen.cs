using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;
using SkiaSharp;
using OpenCvSharp;
using Mat = OpenCvSharp.Mat;
using Point = System.Drawing.Point;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public record DetectedTemplatePoint(MoriTemplateKey MoriTemplateKey, Point Point);

public class DetectCurrentScreen : EffectBase
{
    // private async Task<DetectedTemplatePoint?> DetectCurrentScreenAsync(SKBitmap screenShotSkBitmap,
    //     EmulatorConnection emulator,
    //     MoriTemplateKey moriTemplateKey)
    // {
    //     Logger.Info("Starting detect current screen");
    //     if (TemplateImageDataHelper.IsLoaded &&
    //         TemplateImageDataHelper.TemplateImageData[MoriTemplateKey.StartStartButton].TemplateMat is
    //             { } templateMat)
    //     {
    //         var point = await emulator.GetPointByMatAsync(templateMat, false, screenShotSkBitmap);
    //         if (point is { } bpoint)
    //         {
    //             Logger.Info($"Found template point for key {moriTemplateKey}");
    //
    //             return new DetectedTemplatePoint(MoriTemplateKey: moriTemplateKey, Point: bpoint);
    //         }
    //
    //         Logger.Info($"Not template point for {moriTemplateKey}");
    //     }
    //     else
    //     {
    //         Logger.Info($"TemplateImageDataHelper not loaded");
    //     }
    //
    //     return null;
    // }

    private async Task<DetectedTemplatePoint?> DetectCurrentScreenAsync(
        EmulatorConnection emulator,
        Mat screenshotMat,
        MoriTemplateKey moriTemplateKey)
    {
        try
        {
            Logger.Info("Starting detect current screen");
            await Task.Delay(0);
            if (TemplateImageDataHelper.IsLoaded &&
                TemplateImageDataHelper.TemplateImageData[MoriTemplateKey.StartStartButton].TemplateMat is
                    { } templateMat)
            {
                var point = ImageFinderOpenCvSharp.FindTemplateInScreenshot(screenshotMat, templateMat);
                if (point is { } bpoint)
                {
                    Logger.Info($"Found template point for key {moriTemplateKey}");

                    return new DetectedTemplatePoint(MoriTemplateKey: moriTemplateKey, Point: bpoint);
                }

                Logger.Info($"Not found template point for {moriTemplateKey}");
            }
            else
            {
                Logger.Info($"TemplateImageDataHelper not loaded");
            }

            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to detect current screen for key {moriTemplateKey}");
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
        try
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
            var screenShotOpenCvMat = await emulatorConnection.TakeScreenshotToOpenCVMatAsync();

            if (screenShotOpenCvMat is null) return CoreAction.Empty;

            var tasks = screenToCheck.Select(moriTemplateKey =>
                Observable.FromAsync(
                        () => Task.Run(() =>
                            DetectCurrentScreenAsync(emulatorConnection, screenShotOpenCvMat, moriTemplateKey))
                    )
                    .ObserveOn(Scheduler.Default)
            );

            var result = await tasks
                .Merge()
                .Where(res => res != null)
                .Take(1)
                .FirstOrDefaultAsync();

            if (result is { } detectedTemplatePoint)
            {
                Logger.Info($"Detected template point: {detectedTemplatePoint.Point}");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to process effect detect current screen");
        }

        return CoreAction.Empty;
    }
}