using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects;

public class DetectCurrentScreen : DetectScreenEffectBase
{
    private readonly Enum[] CheckTemplates = [R1999TemplateKey.SkipMovieBtn1,];

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.TriggerScanCurrentScreen,];
    }

    protected override bool IsParallel()
    {
        return false;
    }

    protected override int GetThrottleTime()
    {
        return 4;
    }

    protected override async Task<EventAction> Process(EventAction action)
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

        // Optimize by use one screenshot
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null)
        {
            return CoreAction.Empty;
        }

        var results = await ScanTemplateAsync(
            CheckTemplates,
            emulatorConnection,
            screenshot
        );

        var detectedTemplatePoint = results.FirstOrDefault();

        if (detectedTemplatePoint != null)
        {
            Logger.Info(
                $"Detected template priority for key {detectedTemplatePoint.TemplateKey} with point: {detectedTemplatePoint.Point}"
            );

            return R1999Action.DetectScreen.Create(
                new BaseActionPayload(
                    emulatorConnection.Id,
                    detectedTemplatePoint
                )
            );
        }

        return CoreAction.Empty;
    }
}