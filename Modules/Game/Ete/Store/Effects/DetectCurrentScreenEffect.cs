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
        EteTemplateKey.CloseDailyRwBttn,
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

        EteTemplateKey.HomeSupplyBtn, // detect duoc supply chung to da ok de re-roll
        // EteTemplateKey.SupplyBannerStandard,
        EteTemplateKey.GuideSupplyQuick, // condition to home step 1
        EteTemplateKey.GuideSupplyBack, // condition to home step 1
        EteTemplateKey.GuideSupplyFree, // condition to home step 1
        // EteTemplateKey.ConfirmButton, // TODO: delete
        // EteTemplateKey.ConfirmButton1,  // TODO: delete

        EteTemplateKey.GuideContinueBattle, // condition to home step 2
        // EteTemplateKey.Mission1Header, // TODO: delete
        // EteTemplateKey.MissionStartBtn, // TODO: delete
        EteTemplateKey.Mission1Task, // condition to home step 2
        EteTemplateKey.BackBtn,
    ];

    private readonly Enum[] DoReRollTemplates =
    [
        EteTemplateKey.SkipButton,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.BannerCharDragon,
        EteTemplateKey.Supply10xTextConfirm,
        EteTemplateKey.SupplySkipText,
        EteTemplateKey.SupplyExtraText,
    ];

    private readonly Enum[] RewardScreenTemplates =
    [
        EteTemplateKey.SkipButton,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.CloseDailyRwBttn,
        EteTemplateKey.EmailClaimAllBtn,
    ];

    private readonly Enum[] Step1HomeScreenTemplates =
    [
        EteTemplateKey.SkipButton,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.GuideSupplyQuick,
        EteTemplateKey.SupplyBannerStandard,
        EteTemplateKey.GuideSupplyFree,
        EteTemplateKey.ConfirmButton,
        EteTemplateKey.ConfirmButton1,
        EteTemplateKey.GuideSupplyBack,

        EteTemplateKey.GuideContinueBattle, // condition to home step 2
    ];

    private readonly Enum[] Step2HomeScreenTemplates =
    [
        EteTemplateKey.SkipButton,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.GuideContinueBattle,
        EteTemplateKey.Mission1Header,
        EteTemplateKey.Mission1Task,
        EteTemplateKey.MissionStartBtn, // condition to home step 3
    ];

    private readonly Enum[] Step3HomeScreenTemplates =
    [
        EteTemplateKey.SkipButton,
        EteTemplateKey.HomeSupplyBtn,
        EteTemplateKey.Mission1Header,
        EteTemplateKey.Mission1Task,
        EteTemplateKey.MissionStartBtn,
        EteTemplateKey.CloseDailyRwBttn,
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

        var gameInstance = AppStore.Instance.EteStore.State.GetGameInstance(baseActionPayload.EmulatorId);
        var checkTemplates = CheckTemplatesAll;
        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.Step1HomeScreen)
        {
            checkTemplates = Step1HomeScreenTemplates;
        }
        else if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.Step2HomeScreen)
        {
            checkTemplates = Step2HomeScreenTemplates;
        }
        else if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.Step3HomeScreen)
        {
            checkTemplates = Step3HomeScreenTemplates;
        }
        else if (gameInstance.JobReRollState.ReRollStatus >= EteReRollStatus.GetReward
                 && gameInstance.JobReRollState.ReRollStatus <= EteReRollStatus.GotCodelReward)
        {
            checkTemplates = RewardScreenTemplates;
        }
        else if (gameInstance.JobReRollState.ReRollStatus == EteReRollStatus.DoReRoll)
        {
            Logger.Info("Change to DoReRollTemplates");
            checkTemplates = DoReRollTemplates;
        }

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