using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenDetectedScreenSummonEffect : DetectScreenEffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        R1999TemplateKey.ExitButton,
    ];

    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.DetectScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process WhenDetectedScreenSummonEffect");
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectTemplatePoint detectTemplatePoint)
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


        switch (detectTemplatePoint.TemplateKey)
        {
            case R1999TemplateKey.Chapter5Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.5f, 7.1f));
                isClicked = true;
                break;
            }
            case R1999TemplateKey.HomeMail:
            {
                var gameInstance =
                    AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
                if (gameInstance == null)
                {
                    return CoreAction.Empty;
                }

                if (gameInstance.JobReRollState.ReRollStatus < R1999ReRollStatus.Got1UniCurrentDay)
                {
                    // always save name
                    var isSaveOk = await SaveResult(emulatorConnection);
                    if (!isSaveOk)
                    {
                        break;
                    }

                    await emulatorConnection.ClickPPointAsync(new PPoint(22.8f, 9.2f));
                    await Task.Delay(2000);

                    // get 1 uni
                    await emulatorConnection.ClickPPointAsync(new PPoint(7.7f, 22.7f));
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(24.7f, 66.7f));

                    //  bind account
                    await emulatorConnection.ClickPPointAsync(new PPoint(7.7f, 48.1f));
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(90.5f, 87.1f));

                    return R1999Action.GotDailyReward.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.Got1UniCurrentDay)
                {
                    // lay qua chapter
                    await emulatorConnection.ClickPPointAsync(new PPoint(28.8f, 10.4f));
                    await Task.Delay(2000);

                    // tab1
                    await emulatorConnection.ClickPPointAsync(new PPoint(7.4f, 22.3f));
                    await Task.Delay(1000);

                    var points = await ScanTemplateAsync([R1999TemplateKey.ClaimChapter14Button,], emulatorConnection);
                    if (points.Length > 0)
                    {
                        await emulatorConnection.ClickPPointAsync(new PPoint(46.5f, 81.9f));
                    }

                    await Task.Delay(2000);
                    // tab2
                    await emulatorConnection.ClickPPointAsync(new PPoint(7.3f, 35.6f));
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(22.2f, 83.3f));

                    return R1999Action.GotChapterReward.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.GotChapterReward)
                {
                    // lay qua email
                    await emulatorConnection.ClickPPointAsync(new PPoint(5.8f, 25.9f));
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(16.2f, 86f));
                    await Task.Delay(2000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(16.2f, 86f));
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(3.8f, 6.5f));
                    return R1999Action.GotEmailReward.Create(baseActionPayload);
                }

                if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.GotEmailReward)
                {
                    // click summon
                    await emulatorConnection.ClickPPointAsync(new PPoint(81.7f, 70.3f));
                }

                isClicked = true;
                break;
            }
            case R1999TemplateKey.SummonX1Text:
            {
                // click banner willow
                await emulatorConnection.ClickPPointAsync(new PPoint(10.5f, 39.1f));
                await Task.Delay(500);
                await emulatorConnection.ClickPPointAsync(new PPoint(59.5f, 89.4f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SummonWheel:
            {
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f), 200);
                await Task.Delay(6000);
                await emulatorConnection.ClickPPointAsync(new PPoint(92.3f, 7.1f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.LackUnilogText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(60.9f, 67.2f));
                isClicked = true;
                break;
            }
            case R1999TemplateKey.DontHaveEnoughText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(39f, 58.2f));
                return R1999Action.RollFinished.Create(baseActionPayload);
            }
            default:
            {
                if (_clickOnTemplateKeys.Contains(detectTemplatePoint.TemplateKey))
                {
                    Logger.Info(
                        $"Click template {detectTemplatePoint.TemplateKey} on {detectTemplatePoint.Point}"
                    );
                    await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                    isClicked = true;
                }

                break;
            }
        }

        return isClicked ? R1999Action.ClickedAfterDetectedScreen.Create(baseActionPayload) : CoreAction.Empty;
    }

    protected override bool Filter(EventAction action)
    {
        if (action.Payload is BaseActionPayload baseActionPayload)
        {
            var gameInstance =
                AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
            if (gameInstance is { } gameInstanceData)
            {
                var currentStatus = gameInstanceData.JobReRollState.ReRollStatus;

                return currentStatus >= R1999ReRollStatus.FinishQuest;
            }
        }

        return false;
    }

    protected override ScreenDetectorDataBase GetScreenDetectorDataHelper()
    {
        return R1999ScreenDetectorDataHelper.GetInstance();
    }

    private async Task<bool> SaveResult(EmulatorConnection emulatorConnection)
    {
        var screenshot = await emulatorConnection.TakeScreenshotAsync();

        if (screenshot is null)
        {
            Logger.Error("Failed to take screenshot");
            return false;
        }

        var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(emulatorConnection.Id);

        if (gameInstance == null || gameInstance.JobReRollState.Ordinal == "")
        {
            Logger.Error("Not yet detected ordinal");
            return false;
        }

        await SkiaHelper.SaveScreenshot(
            emulatorConnection,
            ImageHelper.GetImagePath(gameInstance.JobReRollState.Ordinal, "r1999/results/characters"),
            screenshot
        );

        return true;
    }
}