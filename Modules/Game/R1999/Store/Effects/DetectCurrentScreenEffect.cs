using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store.Effects;

public class DetectCurrentScreenEffect : DetectScreenEffectBase
{
    private readonly Enum[] CheckTemplates = [
        R1999TemplateKey.SkipMovieBtn1,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.SignHere,
        R1999TemplateKey.SelectSkill1Text,
        R1999TemplateKey.HoldSkill2Text,
        R1999TemplateKey.LinkSkillNeighboring,
        R1999TemplateKey.SkillTimekeeperText,
        R1999TemplateKey.GuideMeUltimate,
        R1999TemplateKey.SkillUltimateText,
        R1999TemplateKey.ReturnStoryText,
        R1999TemplateKey.Story1Text,
        R1999TemplateKey.Chapter1Button,
        R1999TemplateKey.StartLoss8Button,
        R1999TemplateKey.StartLoss8Button2,
    ];

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
        Logger.Info("Process DetectCurrentScreenEffect");
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
        else
        {
            Logger.Info("No template detected");
        }

        return R1999Action.CouldNotDetectScreen.Create(
            new BaseActionPayload(
                emulatorConnection.Id
            )
        );
    }

    protected override ScreenDetectorDataBase GetScreenDetectorDataHelper()
    {
        return R1999ScreenDetectorDataHelper.GetInstance();
    }
}