using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Store.State;
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
            Logger.Debug($"Starting detect current screen for {moriTemplateKey}");
            if (TemplateImageDataHelper.IsLoaded &&
                TemplateImageDataHelper.TemplateImageData[moriTemplateKey].EmuCVMat is
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
                MoriTemplateKey.TermOfAgreementPopup,
                MoriTemplateKey.StartStartButton,
                MoriTemplateKey.IconChar1,
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
                MoriTemplateKey.GuideSelectTownButton,
                MoriTemplateKey.GuideClickRewardText,
                MoriTemplateKey.GuideClickLevelUpImmediatelyText,
                MoriTemplateKey.GuideClickDownButton,
                MoriTemplateKey.GuideChapter12Text,

                MoriTemplateKey.BossBattleButton,
                MoriTemplateKey.SelectButton,
                MoriTemplateKey.ButtonClaim,

                MoriTemplateKey.PartyInformation,
                MoriTemplateKey.TapToClose,

                // MoriTemplateKey.BeforeChallengeChapterSix
                MoriTemplateKey.BeforeChallengeEnemyPower15,
                MoriTemplateKey.BeforeChallengeEnemyPower16,
                MoriTemplateKey.BeforeChallengeEnemyPower17,
                MoriTemplateKey.BeforeChallengeEnemyPower19,
                MoriTemplateKey.BeforeChallengeEnemyPower112,
                MoriTemplateKey.BeforeChallengeEnemyPower22,

                // MoriTemplateKey.StartSettingButton
                
                MoriTemplateKey.NextCountryButton,
                MoriTemplateKey.CharacterGrowthTabHeader,
                MoriTemplateKey.SkipSceneShotButton,
                MoriTemplateKey.HomeNewPlayerText,
                
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

            return MoriAction.CouldNotDetectMoriScreen.Create(baseActionPayload);
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
            .Throttle(TimeSpan.FromSeconds(3))
            .FilterBaseEligibility(GetForceEligible())
            .Where(action =>
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return false;
                var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

                if (gameInstance == null || (gameInstance.JobType == MoriJobType.ReRoll &&
                                             gameInstance.JobReRollState.ReRollStatus == ReRollStatus.EligibilityLevelCheck ))
                    // tạm thời disable detect để check level
                    // Logger.Info("Pause detect current screen for eligibility level check");
                    return false;

                return true;
            })
            .SelectMany(Process);
    }
}