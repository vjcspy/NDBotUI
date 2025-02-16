using System;
using System.Drawing;
using System.Linq;
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
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateEffectReRoll : EffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.Start,
        ReRollStatus.EligibilityChapterCheck,
        ReRollStatus.EligibilityChapterPassed,
        ReRollStatus.EligibilityLevelPassed,
    ];
    
    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint)
        {
            return CoreAction.Empty;
        }

        // Click
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null)
        {
            return CoreAction.Empty;
        }

        var isClicked = false;
        MoriTemplateKey[] clickOnMoriTemplateKeys =
        [
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.StartStartButton,
            MoriTemplateKey.IconChar1,
            MoriTemplateKey.ChallengeButton,
            MoriTemplateKey.TapToClose,
            MoriTemplateKey.BossBattleButton,
            MoriTemplateKey.SelectButton,
            MoriTemplateKey.ButtonClaim,
            MoriTemplateKey.NextCountryButton,
            MoriTemplateKey.SkipSceneShotButton,
            MoriTemplateKey.InBattleX1, // change to x2
            MoriTemplateKey.NextChapterButton,
        ];

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            // case MoriTemplateKey.IconSpeakBeginningFirst:
            //     await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
            //     await Task.Delay(250);
            //     await emulatorConnection.ClickPPointAsync(new PPoint(49.6f, 81.4f));
            //     isClicked = true;
            //     break;
            case MoriTemplateKey.StartSettingButton:
                break;
            case MoriTemplateKey.LoginClaimButton:
                await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                await Task.Delay(1000);
                // click outside
                await emulatorConnection.ClickPPointAsync(new PPoint(98.4f, 46.3f));
                break;
            case MoriTemplateKey.HomeIconBpText:
                await Task.Delay(1000);
                // click outside
                await emulatorConnection.ClickPPointAsync(new PPoint(44f, 94f));
                break;
            case MoriTemplateKey.TermOfAgreementPopup:
                await emulatorConnection.ClickPPointAsync(new PPoint(30.9f, 31.6f));
                await Task.Delay(1500);
                await emulatorConnection.ClickPPointAsync(new PPoint(57.6f, 83f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectFirstCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(13.7f, 56.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectSecondCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(21.5f, 56.7f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectThirdCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(13.9f, 57.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.TextSelectFourCharToTeam:
                await emulatorConnection.ClickPPointAsync(new PPoint(22.3f, 57.2f));
                isClicked = true;
                break;
            case MoriTemplateKey.PowerLevelUpText:
                await emulatorConnection.ClickPPointAsync(new PPoint(20.8f, 94f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickLevelUpText:
                await emulatorConnection.ClickPPointAsync(new PPoint(75.2f, 82.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickEquipAllText:
                await emulatorConnection.ClickPPointAsync(new PPoint(35.8f, 82.4f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickQuestText:
                await emulatorConnection.ClickPPointAsync(new PPoint(44f, 95f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickTheTownText:
                await emulatorConnection.ClickPPointAsync(new PPoint(50f, 47.3f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideSelectTownButton:
                // select town and next
                await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                await Task.Delay(1500);
                await emulatorConnection.ClickPPointAsync(new PPoint(88.2f, 89.6f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickRewardText:
                await emulatorConnection.ClickPPointAsync(new PPoint(51.6f, 67.3f));
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickLevelUpImmediatelyText:
                await emulatorConnection.ClickPPointAsync(new PPoint(21f, 94.7f));
                await Task.Delay(1500);
                // spam click all char
                await emulatorConnection.ClickPPointAsync(new PPoint(16.9f, 31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(28.5f, 31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(38.9f, 31.8f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(49.9f, 31.8f));
                await Task.Delay(500);
                isClicked = true;
                break;
            case MoriTemplateKey.GuideClickDownButton:
                var pPoint = emulatorConnection.ToPPoint(detectedTemplatePoint.Point);
                if (pPoint != null)
                {
                    await emulatorConnection.ClickPPointAsync(pPoint with { X = pPoint.X + 1, Y = pPoint.Y + 15, });
                }

                isClicked = true;
                break;
            case MoriTemplateKey.GuideChapter12Text:
                await emulatorConnection.ClickPPointAsync(new PPoint(59.6f, 88.5f));
                isClicked = true;
                break;

            case MoriTemplateKey.PartyInformation:
                // Khi không còn action gì mà hiện lên bảng Party Info thì nhấn begin battle
                await emulatorConnection.ClickPPointAsync(new PPoint(85f, 88.2f));
                isClicked = true;
                break;

            case MoriTemplateKey.HomeNewPlayerText:
                // click quest
                await emulatorConnection.ClickPPointAsync(new PPoint(44.3f, 96.0f));
                isClicked = true;
                break;

            case MoriTemplateKey.CharacterGrowthTabHeader:
            {
                if (AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId) is { } gameInstance)
                {
                    if (gameInstance.JobReRollState.CurrentLevel == (int)MoriTemplateKey.BeforeChallengeEnemyPower112)
                    {
                        Logger.Info("Current Chapter Lv 1- 11/12 -> Repeat");
                        // click quest
                        await emulatorConnection.ClickPPointAsync(new PPoint(44.3f, 94.3f));
                        isClicked = true;
                        break;
                    }


                    if (gameInstance.JobReRollState.CurrentLevel < 17 && gameInstance.JobReRollState.CurrentLevel != 0)
                    {
                        // TODO: "Check if need to click on level up";
                        Logger.Info("Current Chapter under Lv17 -> Back to Quest");
                        // click equip all
                        await emulatorConnection.ClickPPointAsync(new PPoint(35.7f, 82.8f));
                        await Task.Delay(1000);

                        // click quest
                        await emulatorConnection.ClickPPointAsync(new PPoint(44.3f, 94.3f));
                        isClicked = true;
                        break;
                    }

                    // Logger.Info("Current Chapter from Lv17 -> Check current status");
                    // if (gameInstance.JobReRollState.ReRollStatus >= ReRollStatus.EligibilityLevelPassed)
                    // {
                    //     Logger.Info("Already pass level check");
                    //     // click equip all
                    //     await emulatorConnection.ClickPPointAsync(new PPoint(35.7f, 82.8f));
                    //     await Task.Delay(1250);
                    //
                    //     // click quest
                    //     await emulatorConnection.ClickPPointAsync(new PPoint(44f, 94.3f));
                    //     await Task.Delay(1250);
                    //     isClicked = true;
                    //     break;
                    // }

                    return MoriAction.EligibilityLevelCheck.Create(baseActionPayload);
                }

                break;
            }

            /* In battle*/
            case MoriTemplateKey.InBattleX2:
                // do nothing
                break;

            default:
            {
                if (clickOnMoriTemplateKeys.Contains(detectedTemplatePoint.MoriTemplateKey))
                {
                    Logger.Info(
                        $"Click template {detectedTemplatePoint.MoriTemplateKey} on {detectedTemplatePoint.Point}"
                    );
                    await emulatorConnection.ClickOnPointAsync(detectedTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }


        return isClicked ? MoriAction.ClickedAfterDetectedMoriScreen.Create(baseActionPayload) : CoreAction.Empty;
    }

    private async Task<Point?> ScanTemplateImage(EmulatorConnection emulatorConnection, MoriTemplateKey templateKey)
    {
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null)
        {
            throw new Exception("Screenshot is null");
        }

        var screenshotEmguMat = screenshot.ToEmguMat();
        // ensure o trong character growth
        if (TemplateImageDataHelper.TemplateImageData[templateKey].EmuCVMat is
            { } templateMat)
        {
            return ImageFinderEmguCV.FindTemplateMatPoint(
                screenshotEmguMat,
                templateMat,
                debugKey: templateKey.ToString(),
                matchValue: 0.9
            );
        }

        return null;
    }
    
    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstance.JobReRollState.ReRollStatus;

                return VALID_STATUS.Contains(currentStatus);
            }
        }

        return false;
    }
}