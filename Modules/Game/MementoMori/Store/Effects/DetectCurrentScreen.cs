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
    private static readonly ReRollStatus[] _DISABLE_DETECT_BY_STATUS =
        [
            ReRollStatus.ResetUserData,
        ];

    private DetectedTemplatePoint? DetectCurrentScreenByEmguCV(
        EmguCVSharp screenshotMat,
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
                    // false,
                    debugKey: moriTemplateKey.ToString(),
                    matchValue: 0.935
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
            MoriAction.TriggerScanCurrentScreen,
        ];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Processing detect current screen effect");
        try
        {
            if (action.Payload is not BaseActionPayload baseActionPayload)
            {
                return CoreAction.Empty;
            }

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

                /* Character growth*/
                MoriTemplateKey.CharacterGrowthPossible,
                MoriTemplateKey.GuideClickLevelUpText,
                MoriTemplateKey.GuideClickEquipAllText,
                MoriTemplateKey.GuideClickQuestText,
                MoriTemplateKey.GuideClickTheTownText,
                MoriTemplateKey.GuideSelectTownButton,
                MoriTemplateKey.GuideClickRewardText,
                MoriTemplateKey.GuideClickLevelUpImmediatelyText,
                MoriTemplateKey.GuideClickDownButton,
                MoriTemplateKey.GuideChapter12Text,
                MoriTemplateKey.GuideChapter12Text1,

                MoriTemplateKey.BossBattleButton,
                MoriTemplateKey.SelectButton,
                MoriTemplateKey.ButtonClaim,

                MoriTemplateKey.PartyInformation,
                MoriTemplateKey.CharacterTabHeader,
                MoriTemplateKey.TapToClose,
                MoriTemplateKey.NextChapterButton,

                // MoriTemplateKey.BeforeChallengeChapterSix
                MoriTemplateKey.BeforeChallengeEnemyPower15,
                MoriTemplateKey.BeforeChallengeEnemyPower16,
                MoriTemplateKey.BeforeChallengeEnemyPower17,
                MoriTemplateKey.BeforeChallengeEnemyPower18,
                MoriTemplateKey.BeforeChallengeEnemyPower19,
                MoriTemplateKey.BeforeChallengeEnemyPower111,
                MoriTemplateKey.BeforeChallengeEnemyPower112,
                MoriTemplateKey.BeforeChallengeEnemyPower21,
                MoriTemplateKey.BeforeChallengeEnemyPower22,

                MoriTemplateKey.StartSettingButton, // Mục đích là không làm gì trong lúc load ở start

                MoriTemplateKey.NextCountryButton,
                MoriTemplateKey.CharacterGrowthTabHeader,
                MoriTemplateKey.SkipSceneShotButton,
                MoriTemplateKey.HomeNewPlayerText,

                /*In battle*/
                MoriTemplateKey.InBattleX1,
                MoriTemplateKey.InBattleX2,

                /*Home*/
                MoriTemplateKey.LoginClaimButton,
                MoriTemplateKey.HomeIconBpText,
                MoriTemplateKey.ReturnToTitleButton,
                
                // Reset
                MoriTemplateKey.ResetGameDataButton,
                MoriTemplateKey.ResetGameDataHeader,
                MoriTemplateKey.ConfirmGameDataResetHeader,
            ];

            var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            if (emulatorConnection == null)
            {
                return CoreAction.Empty;
            }

            // Optimize by use one screenshot
            var screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null)
            {
                return CoreAction.Empty;
            }

            var screenshotEmguMat = screenshot.ToEmguMat();

            var tasks = screenToCheck
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
                .ToList();

            Logger.Info($"Found {result.Count} detected template points");

            var detectedTemplatePoint = result
                .OrderBy(
                    point =>
                        TemplateImageDataHelper
                            .TemplateImageData[point!.MoriTemplateKey]
                            .GetPriority(baseActionPayload.EmulatorId)
                )
                .FirstOrDefault();

            if (detectedTemplatePoint != null)
            {
                Logger.Info(
                    $"Detected template priority for key {detectedTemplatePoint.MoriTemplateKey} with point: {detectedTemplatePoint.Point}"
                );

                return MoriAction.DetectedMoriScreen.Create(
                    new BaseActionPayload(
                        emulatorConnection.Id,
                        detectedTemplatePoint
                    )
                );
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
            .GroupBy(
                action =>
                {
                    if (action.Payload is BaseActionPayload baseActionPayload)
                    {
                        return baseActionPayload.EmulatorId;
                    }

                    return Guid.Empty.ToString(); // Trường hợp không có Id, đảm bảo không lỗi
                }
            )
            .SelectMany(
                groupedStream =>
                    groupedStream
                        .Throttle(TimeSpan.FromSeconds(3)) // Throttle theo từng EmulatorId
                        .FilterBaseEligibility(GetForceEligible())
            )
            .Where(
                action =>
                {
                    if (action.Payload is not BaseActionPayload baseActionPayload)
                    {
                        return false;
                    }

                    var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

                    if (
                        gameInstance == null
                        || (
                            gameInstance.JobType == MoriJobType.ReRoll
                            && _DISABLE_DETECT_BY_STATUS.Contains(gameInstance.JobReRollState.ReRollStatus)
                        )
                    )
                    {
                        return false;
                    }

                    return true;
                }
            )
            .SelectMany(ProcessWrapper);
    }
}