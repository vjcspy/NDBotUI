﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
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
    private readonly MoriTemplateKey[] _getPresentsTemplate =
    [
        MoriTemplateKey.HomePresentsIcon,
        MoriTemplateKey.ErrorHeaderPopup,
        MoriTemplateKey.LoginClaimButton,
        MoriTemplateKey.LoginClaimButton,
        MoriTemplateKey.ButtonClaim,
        MoriTemplateKey.BossBattleButton,
        MoriTemplateKey.PresentsBoxHeader,
        MoriTemplateKey.BannerNewbie,
        MoriTemplateKey.InvokeCloseButton,
        MoriTemplateKey.NotHaveEnoughDiamondText,
        MoriTemplateKey.SpendDiamondsInvoke,
    ];

    private readonly MoriTemplateKey[] _screenToCheck =
    [
        MoriTemplateKey.ErrorHeaderPopup,
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
        MoriTemplateKey.BeforeChallengeEnemyPower23,

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
        MoriTemplateKey.ReturnToTitleHeader,
        MoriTemplateKey.ResetGameDataButton,
        MoriTemplateKey.ResetGameDataHeader,
        MoriTemplateKey.ConfirmGameDataResetHeader,
        MoriTemplateKey.DownloadUpdateButton,

        // Link
        MoriTemplateKey.EnterYourLinkAccountText,
        MoriTemplateKey.PerformAccountLink,
        MoriTemplateKey.SetLinkPassword,
        MoriTemplateKey.EnterLinkInfo,
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


            var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            if (emulatorConnection == null)
            {
                return CoreAction.Empty;
            }

            var gameInstance =
                AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

            if (gameInstance == null)
            {
                return CoreAction.Empty;
            }

            var templateChecks = _screenToCheck;
            if (gameInstance.JobReRollState.ReRollStatus is ReRollStatus.GetPresents or ReRollStatus.GotPresents or ReRollStatus.RollX1)
            {
                templateChecks = _getPresentsTemplate;
            }

            // Optimize by use one screenshot
            var screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null)
            {
                return CoreAction.Empty;
            }

            var screenshotEmguMat = screenshot.ToEmguMat();

            var semaphore = new SemaphoreSlim(2); // 2 thread tối đa

            var tasks = templateChecks
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

    protected override bool IsParallel()
    {
        return false;
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload)
        {
            return false;
        }

        var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

        if (gameInstance == null)
        {
            return false;
        }

        return true;
    }

    protected override int GetThrottleTime()
    {
        return 4;
    }
}