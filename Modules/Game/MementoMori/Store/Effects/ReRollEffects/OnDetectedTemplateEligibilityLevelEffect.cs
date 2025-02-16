using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class OnDetectedTemplateEligibilityLevelEffect : ScanTemplateEffectBase
{
    private static readonly ReRollStatus[] VALID_STATUS =
    [
        ReRollStatus.EligibilityLevelCheck,
        ReRollStatus.EligibilityLevelCheckOnChar,
    ];

    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen,];
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return VALID_STATUS.Contains(currentStatus);
            }
        }

        return false;
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
            MoriTemplateKey.CharacterGrowthPossible,
        ];

        var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            case MoriTemplateKey.CharacterTabHeader:
            {
                switch (gameInstance.JobReRollState.LevelUpCharPosition)
                {
                    case 0:
                        await emulatorConnection.ClickPPointAsync(new PPoint(16.6f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 0, }
                        );
                    case 1:
                        await emulatorConnection.ClickPPointAsync(new PPoint(28.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 1, }
                        );
                    case 2:
                        await emulatorConnection.ClickPPointAsync(new PPoint(39.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 2, }
                        );
                    case 3:
                        await emulatorConnection.ClickPPointAsync(new PPoint(49.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 3, }
                        );
                }

                break;
            }

            case MoriTemplateKey.CharacterGrowthTabHeader:
            {
                // scan and level up
                await LevelUpChar(emulatorConnection);
                return MoriAction.EligibilityLevelCheckOnCharOk.Create(baseActionPayload);
            }

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

    private async Task LevelUpChar(EmulatorConnection emulatorConnection)
    {
        MatchValue = 0.9f;
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null)
        {
            throw new Exception("Could not take screenshot");
        }

        // var screenshotEmguMat = screenshot.ToEmguMat();
        // // ensure o trong character growth
        // var point = await FindTemplate(MoriTemplateKey.CharacterGrowthTabHeader, screenshotEmguMat);
        //
        // if (point is null)
        // {
        //     throw new Exception("Not in Character Growth Tab");
        // }

        MoriTemplateKey[] lvToCheck =
        [
            MoriTemplateKey.CharacterLevelOneText, MoriTemplateKey.CharacterLevelTwoText,
            MoriTemplateKey.CharacterLevelThreeText, MoriTemplateKey.CharacterLevelFourText,
            MoriTemplateKey.CharacterLevelFiveText, MoriTemplateKey.CharacterLevelSixText,
        ];

        var countLevelUp = 0;

        while (countLevelUp < 7)
        {
            var lv7Point = await ScanTemplateAsync(
                [MoriTemplateKey.CharacterLevelSevenText,],
                emulatorConnection,
                screenshot
            );

            // char đã lv7
            if (lv7Point.Length > 0)
            {
                Logger.Info("Character already lv 7");

                // equip all
                await emulatorConnection.ClickPPointAsync(new PPoint(36.3f, 82.4f));
                await Task.Delay(500);

                // back
                await emulatorConnection.ClickPPointAsync(new PPoint(21.3f, 94.5f));
                return;
            }

            var isBellowLv7 = await ScanTemplateAsync(
                lvToCheck,
                emulatorConnection,
                screenshot
            );

            if (isBellowLv7.Length == 0)
            {
                throw new Exception("Unknown current level");
            }

            // level up
            Logger.Info("Click Level Up");
            emulatorConnection.ClickPPoint(new PPoint(76.7f, 82.9f));
            countLevelUp += 1;
            await Task.Delay(1000);

            // refresh level screen
            screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null)
            {
                throw new Exception("Could not take screen shot");
            }
        }
    }
}