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

public class WhenDetectedScreenSaveResultEffect : DetectScreenEffectBase
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
        Logger.Info(">>Process WhenDetectedScreenSaveResultEffect");
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
            case R1999TemplateKey.HomeMail:
                await emulatorConnection.ClickPPointAsync(new PPoint(89.6f, 56.6f));
                isClicked = true;
                break;

            case R1999TemplateKey.CharacterLevelText:
            case R1999TemplateKey.CharacterLevelText1:
                await Task.Delay(1000);
                var isSaveOk = await SaveResult(emulatorConnection);
                if (isSaveOk)
                {
                    await Task.Delay(1000);
                    await emulatorConnection.ClickPPointAsync(new PPoint(11.4f, 6f));
                    return R1999Action.SaveResultOk.Create(baseActionPayload);
                }

                break;
            case R1999TemplateKey.SummonX1Text:
            {
                // click exit button
                await emulatorConnection.ClickPPointAsync(new PPoint(10.9f, 6.5f));
                isClicked = true;
                break;
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

                return currentStatus == R1999ReRollStatus.RollFinished;
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