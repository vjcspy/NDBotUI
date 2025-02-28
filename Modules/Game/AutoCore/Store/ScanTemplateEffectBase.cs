using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using Emgu.CV;
using LanguageExt;
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
        // Tạo SemaphoreSlim cho phép tối đa 2 thread chạy đồng thời
        var semaphore = new SemaphoreSlim(3); // 2 thread tối đa


        var tasks = templateKeys
            .Select(
                async templateKey =>
                {
                    // Chờ đến khi có thread trống
                    await semaphore.WaitAsync();

                    try
                    {
                        // Gọi hàm DetectCurrentScreenByEmguCV trong một thread mới
                        return await Task.Run(() => DetectCurrentScreenByEmguCV(screenshotEmguMat, templateKey));
                    }
                    finally
                    {
                        // Giải phóng semaphore để cho phép thread khác thực thi
                        semaphore.Release();
                    }
                }
            )
            .ToArray();

        // Chạy tất cả các task đồng thời và lấy kết quả
        var result = await Task
            .WhenAll(tasks) // Chạy đồng thời tất cả các task
            .Select(res => res.Where(a => a != null));

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