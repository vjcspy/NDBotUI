using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

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
        Logger.Info("Process on eligibility level check");
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
            MoriTemplateKey.SkipMovieButton,
            MoriTemplateKey.SkipSceneShotButton,
            MoriTemplateKey.IconChar1, // cho vào hơi vô lý nhưng để đảm bảo không bị lỗi khi không detect được
        ];

        var gameInstance = AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId);

        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        switch (detectedTemplatePoint.MoriTemplateKey)
        {
            case MoriTemplateKey.GuideClickDownButton:
            {
                var pPoint = emulatorConnection.ToPPoint(detectedTemplatePoint.Point);
                if (pPoint != null)
                {
                    await emulatorConnection.ClickPPointAsync(pPoint with { X = pPoint.X + 1, Y = pPoint.Y + 15, });
                }

                isClicked = true;
                break;
            }
            case MoriTemplateKey.CharacterTabHeader:
            {
                switch (gameInstance.JobReRollState.LevelUpCharPosition)
                {
                    case 0:
                        Logger.Info("Click to level up char 1");
                        await emulatorConnection.ClickPPointAsync(new PPoint(16.6f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 0, }
                        );
                    case 1:
                        Logger.Info("Click to level up char 2");
                        await emulatorConnection.ClickPPointAsync(new PPoint(28.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 1, }
                        );
                    case 2:
                        Logger.Info("Click to level up char 3");
                        await emulatorConnection.ClickPPointAsync(new PPoint(39.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 2, }
                        );
                    case 3:
                        Logger.Info("Click to level up char 4");
                        await emulatorConnection.ClickPPointAsync(new PPoint(49.1f, 33.1f));
                        return MoriAction.EligibilityLevelCheckOnChar.Create(
                            baseActionPayload with { Data = 3, }
                        );
                    default:
                        Logger.Error($"Invalid level up char position {gameInstance.JobReRollState.LevelUpCharPosition}" );
                        break;
                }
                break;
            }

            case MoriTemplateKey.CharacterGrowthTabHeader:
            {
                // scan and level up
                var isLevelUpOk = await LevelUpChar(emulatorConnection);
                return isLevelUpOk ? MoriAction.EligibilityLevelCheckOnCharOk.Create(baseActionPayload) : CoreAction.Empty;
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

    private async Task<bool> LevelUpChar(EmulatorConnection emulatorConnection)
    {
        Logger.Info("Start level up char");
        MatchValue = 0.9f;
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null)
        {
            Logger.Error("Could not take screen shot");
            return false;
        }

        // ensure o trong character growth
        var characterGrowthTab = await ScanTemplateAsync(
            [MoriTemplateKey.CharacterGrowthTabHeader,],
            emulatorConnection,
            screenshot
        );

        if (characterGrowthTab.Length == 0)
        {
            Logger.Warn("Not in character growth tab");
            return false;
        }
        
        // equip all
        await emulatorConnection.ClickPPointAsync(new PPoint(36.3f, 82.4f));
        await Task.Delay(500);

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

                // back
                Logger.Info("Back to Chracter Tab");
                await emulatorConnection.ClickPPointAsync(new PPoint(21.3f, 94.5f));
                return true;
            }

            var isBellowLv7 = await ScanTemplateAsync(
                lvToCheck,
                emulatorConnection,
                screenshot
            );

            if (isBellowLv7.Length == 0)
            {
                Logger.Error("Unknown current level");
                return false;
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
                Logger.Error("Could not take screen shot");
                return false;
            }
        }
        
        // Vì có thể hết resource
        return true;
    }
}