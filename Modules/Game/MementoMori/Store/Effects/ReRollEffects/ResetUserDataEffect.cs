using System;
using System.Drawing;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class ResetUserDataEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.ResetUserData,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Process save result");
        if (action.Payload is not BaseActionPayload baseActionPayload)
        {
            return CoreAction.Empty;
        }

        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);
        if (emulatorConnection == null)
        {
            Logger.Error("No emulator connection found");
            return await WhenDoneOrError(baseActionPayload);
        }

        // click home
        emulatorConnection.ClickPPoint(new PPoint(9.1f, 94.6f));
        await Task.Delay(4000);

        var homeNewPlayerHeaderPoint = await ScanTemplateImage(emulatorConnection, MoriTemplateKey.HomeNewPlayerText);

        if (homeNewPlayerHeaderPoint == null)
        {
            Logger.Error("No home new player found");
            return await WhenDoneOrError(baseActionPayload);
        }

        // click menu
        emulatorConnection.ClickPPoint(new PPoint(96.7f, 3.5f));
        await Task.Delay(1500);

        var returnToTileButtonPoint = await ScanTemplateImage(emulatorConnection, MoriTemplateKey.ReturnToTitleButton);
        if (returnToTileButtonPoint == null)
        {
            Logger.Error("No return to tile button found");
            return await WhenDoneOrError(baseActionPayload);
        }

        emulatorConnection.ClickOnPoint((Point)returnToTileButtonPoint);
        await Task.Delay(1000);
        emulatorConnection.ClickPPoint(new PPoint(58.1f, 61.1f));
        await Task.Delay(10000);

        var settingButtonPoint = await ScanTemplateImage(emulatorConnection, MoriTemplateKey.StartSettingButton);
        if (settingButtonPoint == null)
        {
            Logger.Error("No start setting button found");
            return await WhenDoneOrError(baseActionPayload);
        }

        emulatorConnection.ClickOnPoint((Point)settingButtonPoint);
        await Task.Delay(1000);

        // click reset game data button
        emulatorConnection.ClickPPoint(new PPoint(40.2f, 53.0f));
        await Task.Delay(1000);

        // click reset
        emulatorConnection.ClickPPoint(new PPoint(57.8f, 68.4f));
        await Task.Delay(1000);
        // click confirm
        emulatorConnection.ClickPPoint(new PPoint(58.3f, 61.9f));
        await Task.Delay(5000);

        return await WhenDoneOrError(baseActionPayload);
    }

    private async Task<EventAction> WhenDoneOrError(BaseActionPayload baseActionPayload)
    {
        Logger.Error("Could not find character tab header, toggle auto");
        RxEventManager.Dispatch(
            MoriAction.ToggleStartStopMoriReRoll.Create(
                new BaseActionPayload(baseActionPayload.EmulatorId)
            )
        );
        await Task.Delay(3000);
        RxEventManager.Dispatch(
            MoriAction.ToggleStartStopMoriReRoll.Create(
                new BaseActionPayload(baseActionPayload.EmulatorId)
            )
        );

        return CoreAction.Empty;
    }

    private async Task<Point?> ScanTemplateImage(
        EmulatorConnection emulatorConnection,
        MoriTemplateKey templateKey,
        Framebuffer? screenshot = null
    )
    {
        if (screenshot == null)
        {
            screenshot = await emulatorConnection.TakeScreenshotAsync();
        }

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
}