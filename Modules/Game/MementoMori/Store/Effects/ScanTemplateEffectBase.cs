using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using Emgu.CV;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public abstract class ScanTemplateEffectBase : EffectBase
{
    protected async Task<DetectedTemplatePoint[]> ScanTemplateAsync(
        MoriTemplateKey[] templateKeys,
        EmulatorConnection emulatorConnection,
        Framebuffer? screenshot
    )
    {
        // Optimize by use one screenshot
        screenshot ??= await emulatorConnection.TakeScreenshotAsync();

        if (screenshot == null)
        {
            Logger.Error("Failed to take screenshot");

            return [];
        }

        var screenshotEmguMat = screenshot.ToEmguMat();
        var tasks = templateKeys
            .Select(
                moriTemplateKey =>
                    Observable
                        .FromAsync(
                            () => Task.Run(() => DetectCurrentScreenByEmguCV(screenshotEmguMat, moriTemplateKey))
                        )
                        .SubscribeOn(Scheduler.Default)
            );

        var result = await tasks
            .Merge()
            .Where(res => res != null)
            .ToArray();

        var resultOrdered = result.OrderBy(
            point =>
                TemplateImageDataHelper
                    .TemplateImageData[point!.MoriTemplateKey]
                    .GetPriority(emulatorConnection.Id)
        );

        return resultOrdered.ToArray()!;
    }

    private DetectedTemplatePoint? DetectCurrentScreenByEmguCV(
        Mat screenshotMat,
        MoriTemplateKey moriTemplateKey
    )
    {
        try
        {
            Logger.Debug($"Starting detect current screen for {moriTemplateKey}");
            if (TemplateImageDataHelper.IsLoaded
                && TemplateImageDataHelper.TemplateImageData[moriTemplateKey].EmuCVMat is
                    { } templateMat)
            {
                var point = ImageFinderEmguCV.FindTemplateMatPoint(
                    screenshotMat,
                    templateMat,
                    false,
                    debugKey: moriTemplateKey.ToString(),
                    matchValue: 0.85
                    // markedScreenshotFileName: $"{moriTemplateKey.ToString()}.png"
                );
                if (point is { } bpoint)
                {
                    Logger.Debug($"Found template point for key {moriTemplateKey}");

                    return new DetectedTemplatePoint(moriTemplateKey, bpoint);
                }

                Logger.Debug($"Not found template point for {moriTemplateKey}");
            }
            else
            {
                Logger.Error($"TemplateImageDataHelper not loaded for key {moriTemplateKey}");
            }

            return null;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to detect current screen for key {moriTemplateKey}");
        }

        return null;
    }
}