using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using EmguCVSharp = Emgu.CV.Mat;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects;

public class DetectCurrentScreen : EffectBase
{
    private DetectedTemplatePoint? DetectCurrentScreenByEmguCV(
        EmguCVSharp screenshotMat,
        MoriTemplateKey moriTemplateKey)
    {
        try
        {
            Logger.Info($"Starting detect current screen for {moriTemplateKey}");
            if (TemplateImageDataHelper.IsLoaded &&
                TemplateImageDataHelper.TemplateImageData[moriTemplateKey].EmuCVMat is
                    { } templateMat)
            {
                var point = ImageFinderEmguCV.FindTemplateMatPoint(
                    screenshotMat,
                    templateMat,
                    shouldResize: true
                    // markedScreenshotFileName: $"{moriTemplateKey.ToString()}.png"
                );
                if (point is { } bpoint)
                {
                    Logger.Info($"Found template point for key {moriTemplateKey}");

                    return new DetectedTemplatePoint(moriTemplateKey, bpoint);
                }

                Logger.Info($"Not found template point for {moriTemplateKey}");
            }
            else
            {
                Logger.Info($"TemplateImageDataHelper not loaded for key {moriTemplateKey}");
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
            MoriAction.EligibilityCheck,
            MoriAction.ClickedAfterDetectedMoriScreen,
        ];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Processing detect current screen effect");
        try
        {
            if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;

            MoriTemplateKey[] screenToCheck =
            [
                MoriTemplateKey.StartStartButton,
                MoriTemplateKey.IconSpeakBeginningFirst,
                // MoriTemplateKey.StartSettingButton
            ];

            var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            if (emulatorConnection == null) return CoreAction.Empty;

            // Optimize by use one screenshot
            var screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null) return CoreAction.Empty;

            var screenshotEmguMat = screenshot.ToEmguMat();

            var cts = new CancellationTokenSource();
            var cancelSignal = new Subject<DetectedTemplatePoint>(); // Phát tín hiệu khi tìm thấy kết quả đầu tiên

            var tasks = screenToCheck
                .Select(moriTemplateKey =>
                    Observable.FromAsync(
                            () => Task.Run(() => DetectCurrentScreenByEmguCV(screenshotEmguMat, moriTemplateKey),
                                cts.Token)
                        )
                        .SubscribeOn(Scheduler.Default)
                        .TakeUntil(cancelSignal)
                );

            var result = await tasks
                .Merge()
                .Where(res => res != null)
                .Take(1)
                .Do(detected =>
                {
                    cts.Cancel(); // Hủy tất cả task chưa hoàn thành
                    if (detected != null) cancelSignal.OnNext(detected); // Phát tín hiệu để dừng
                    cancelSignal.OnCompleted(); // Đóng Subject
                })
                .FirstOrDefaultAsync();

            if (result is { } detectedTemplatePoint)
            {
                Logger.Info(
                    $"Detected template {detectedTemplatePoint.MoriTemplateKey} with point: {detectedTemplatePoint.Point}");

                return MoriAction.DetectedMoriScreen.Create(new BaseActionPayload(emulatorConnection.Id,
                    detectedTemplatePoint));
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to process effect detect current screen");
        }

        return CoreAction.Empty;
    }
    
    [Effect]
    public override RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(GetAllowEventActions())
            .FilterBaseEligibility(GetForceEligible())
            .Throttle(TimeSpan.FromSeconds(1)) 
            .SelectMany(Process);
    }
}