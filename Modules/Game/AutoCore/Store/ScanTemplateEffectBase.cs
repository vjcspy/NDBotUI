using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using Emgu.CV;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;

namespace NDBotUI.Modules.Game.AutoCore.Store;

public record DetectTemplatePoint(Enum TemplateKey, Point Point);

public abstract class DetectScreenEffectBase : EffectBase
{
    protected float MatchValue { get; set; } = 0.85f;

    protected virtual ScreenDetectorDataBase GetScreenDetectorDataHelper()
    {
        throw new NotImplementedException();
    }

    protected async Task<DetectTemplatePoint[]> ScanTemplateAsync(
        Enum[] templateKeys,
        EmulatorConnection emulatorConnection,
        Framebuffer? screenshot = null
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
                templateKey =>
                    Observable
                        .FromAsync(
                            () => Task.Run(() => DetectCurrentScreenByEmguCV(screenshotEmguMat, templateKey))
                        )
                        .SubscribeOn(Scheduler.Default)
            );

        var result = await tasks
            .Merge(5)
            .Where(res => res != null)
            .ToArray();

        var resultOrdered = result.OrderBy(
            point =>
                GetScreenDetectorDataHelper()
                    .TemplateImageData[point!.TemplateKey]
                    .GetPriority(emulatorConnection.Id)
        );

        return resultOrdered.ToArray()!;
    }

    private DetectTemplatePoint? DetectCurrentScreenByEmguCV(
        Mat screenshotMat,
        Enum templateKey
    )
    {
        try
        {
            Logger.Debug($"Starting detect current screen for {templateKey}");
            if (GetScreenDetectorDataHelper()
                    .IsLoaded
                && GetScreenDetectorDataHelper()
                        .TemplateImageData[templateKey].EmuCVMat is
                    { } templateMat)
            {
                var point = ImageFinderEmguCV.FindTemplateMatPoint(
                    screenshotMat,
                    templateMat,
                    // false,
                    debugKey: templateKey.ToString(),
                    matchValue: MatchValue
                    // markedScreenshotFileName: $"{moriTemplateKey.ToString()}.png"
                );
                if (point is { } bpoint)
                {
                    Logger.Debug($"Found template point for key {templateKey}");

                    return new DetectTemplatePoint(templateKey, bpoint);
                }

                Logger.Debug($"Not found template point for {templateKey}");
            }
            else
            {
                Logger.Error($"TemplateImageDataHelper not loaded for key {templateKey}");
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