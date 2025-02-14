using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;
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
                    shouldResize: false
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

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return
        [
            MoriAction.TriggerScanCurrentScreen
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
                MoriTemplateKey.ChallengeButton,
                MoriTemplateKey.SkipMovieButton,
                
                MoriTemplateKey.TextSelectFirstCharToTeam,
                MoriTemplateKey.TextSelectSecondCharToTeam,
                MoriTemplateKey.TextSelectThirdCharToTeam,
                MoriTemplateKey.TextSelectFourCharToTeam,
                
                MoriTemplateKey.PowerLevelUpText,
                MoriTemplateKey.GuideClickLevelUpText,
                MoriTemplateKey.GuideClickEquipAllText,
                MoriTemplateKey.GuideClickQuestText,
                MoriTemplateKey.GuideClickTheTownText,
                
                MoriTemplateKey.BossBattleButton,
                
                MoriTemplateKey.PartyInformation,
                MoriTemplateKey.TapToClose,
                
                // MoriTemplateKey.StartSettingButton
            ];

            var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            if (emulatorConnection == null) return CoreAction.Empty;

            // Optimize by use one screenshot
            var screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null) return CoreAction.Empty;

            var screenshotEmguMat = screenshot.ToEmguMat();

            var tasks = screenToCheck
                .Select(moriTemplateKey =>
                    Observable.FromAsync(
                            () => Task.Run(() => DetectCurrentScreenByEmguCV(screenshotEmguMat, moriTemplateKey))
                        )
                        .SubscribeOn(Scheduler.Default)
                );

            var result = await tasks
                .Merge()
                .Where(res => res != null)
                .ToList();

            Logger.Info($"Found {result.Count} detected template points");

            var detectedTemplatePoint = result
                .OrderBy(point => TemplateImageDataHelper.TemplateImageData[point.MoriTemplateKey].Priority)
                .FirstOrDefault();

            if (detectedTemplatePoint != null)
            {
                Logger.Info(
                    $"Detected template priority for key {detectedTemplatePoint.MoriTemplateKey} with point: {detectedTemplatePoint.Point}");

                return MoriAction.DetectedMoriScreen.Create(new BaseActionPayload(emulatorConnection.Id,
                    detectedTemplatePoint));
            }
            else
            {
                return MoriAction.CouldNotDetectMoriScreen.Create(baseActionPayload);
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