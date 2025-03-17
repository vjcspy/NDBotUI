using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.Ete.Helper;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store.Effects;

public class DetectCurrentScreenEffect : DetectScreenEffectBase
{
    private readonly Enum[] CheckTemplatesAll =
    [
        EteTemplateKey.StartCloseDialogButton,
        EteTemplateKey.LoginAgreementText,
        EteTemplateKey.GraphicQualityText,

        EteTemplateKey.SkipButton,
        EteTemplateKey.ClickStrike,

        // In battle
        EteTemplateKey.GuideJoystick,
        EteTemplateKey.GuideNormalAttk,
        EteTemplateKey.GuideIntrinsicSkill,
        EteTemplateKey.InBattleCharIcon1,
        EteTemplateKey.InBattleCharIcon11,
        EteTemplateKey.GuideEnemy2Assault,
        EteTemplateKey.GuideEnemy2Assault2,

        EteTemplateKey.GuideNoPrblText,
        EteTemplateKey.GuideEnterName,
    ];


    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EteAction.TriggerScanCurrentScreen,];
    }

    protected override bool IsParallel()
    {
        return false;
    }

    protected override int GetThrottleTime()
    {
        return 5;
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

        var gameInstance = AppStore.Instance.EteStore.State.GetGameInstance(baseActionPayload.EmulatorId);
        var checkTemplates = CheckTemplatesAll;
        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        // if (gameInstance.JobReRollState.ReRollStatus >= R1999ReRollStatus.SaveResultOk)
        // {
        //     Logger.Info("Change to RenewAccountTemplates");
        //     checkTemplates = RenewAccountTemplates;
        // }

        // Optimize by use one screenshot
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null)
        {
            return CoreAction.Empty;
        }

        var results = await ScanTemplateAsync(
            checkTemplates,
            emulatorConnection,
            screenshot
        );

        var detectedTemplatePoint = results.FirstOrDefault();

        if (detectedTemplatePoint != null)
        {
            Logger.Info(
                $"Detected template priority for key {detectedTemplatePoint.TemplateKey} with point: {detectedTemplatePoint.Point}"
            );

            return EteAction.DetectScreen.Create(
                new BaseActionPayload(
                    emulatorConnection.Id,
                    detectedTemplatePoint
                )
            );
        }

        Logger.Info("No template detected");

        return EteAction.CouldNotDetectScreen.Create(
            new BaseActionPayload(
                emulatorConnection.Id
            )
        );
    }

    protected override ScreenDetectorDataBase GetScreenDetectorDataHelper()
    {
        return EteScreenDetectorDataHelper.GetInstance();
    }
}