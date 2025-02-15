using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Models;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Typing;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class SaveResultEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Process save result");
        if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;

        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);
        if (emulatorConnection == null)
        {
            Logger.Error("No emulator connection found");
            return await WhenError(baseActionPayload);
        }

        // click outside
        emulatorConnection.ClickPPoint(new PPoint(84.4f, 20.4f));
        await Task.Delay(500);

        // click into character
        emulatorConnection.ClickPPoint(new PPoint(20.8f, 94.2f));
        await Task.Delay(1500);

        var screenshot = await emulatorConnection.TakeScreenshotAsync();

        if (screenshot is null) return await WhenError(baseActionPayload);

        var characterTabPoint = await ScanTemplateImage(
            emulatorConnection,
            MoriTemplateKey.CharacterTabHeader,
            screenshot);

        if (characterTabPoint != null)
            await SkiaHelper.SaveScreenshot(emulatorConnection, ImageHelper.GetImagePath("character", "results/characters"),
                screenshot);
        else
            return await WhenError(baseActionPayload);


        return MoriAction.ResetUserData.Create(baseActionPayload);
    }

    private async Task<EventAction> WhenError(BaseActionPayload baseActionPayload)
    {
        Logger.Error("Could not find character tab header, toggle auto");
        RxEventManager.Dispatch(MoriAction.ToggleStartStopMoriReRoll.Create(
            new BaseActionPayload(baseActionPayload.EmulatorId)));
        await Task.Delay(3000);
        RxEventManager.Dispatch(MoriAction.ToggleStartStopMoriReRoll.Create(
            new BaseActionPayload(baseActionPayload.EmulatorId)));

        return CoreAction.Empty;
    }

    [Effect]
    public override RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(GetAllowEventActions())
            .FilterBaseEligibility(GetForceEligible())
            .Where(action =>
            {
                if (action.Payload is not BaseActionPayload baseActionPayload) return false;

                if (baseActionPayload.Data is not DetectedTemplatePoint detectedTemplatePoint) return false;

                return detectedTemplatePoint.MoriTemplateKey == MoriTemplateKey.BeforeChallengeEnemyPower22;
            })
            .SelectMany(Process);
    }

    private async Task<Point?> ScanTemplateImage(EmulatorConnection emulatorConnection, MoriTemplateKey templateKey,
        Framebuffer? screenshot = null)
    {
        if (screenshot == null) screenshot = await emulatorConnection.TakeScreenshotAsync();

        if (screenshot is null) throw new Exception("Screenshot is null");
        var screenshotEmguMat = screenshot.ToEmguMat();
        // ensure o trong character growth
        if (TemplateImageDataHelper.TemplateImageData[templateKey].EmuCVMat is
            { } templateMat)
            return ImageFinderEmguCV.FindTemplateMatPoint(
                screenshotEmguMat,
                templateMat,
                debugKey: templateKey.ToString(),
                matchValue: 0.9
            );

        return null;
    }
}