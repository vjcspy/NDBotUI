using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Helper;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;
using NLog;

namespace NDBotUI.Modules.Game.R1999.Store.Effects;

public class DetectCurrentScreenEffect : DetectScreenEffectBase
{
    private readonly Enum[] CheckTemplatesAll = [
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
        R1999TemplateKey.AccelerateBattleText,
        R1999TemplateKey.ChooseEnemyText,
        R1999TemplateKey.SummonText,
        R1999TemplateKey.SummonWheel,
        R1999TemplateKey.CheckCrewText,
        R1999TemplateKey.HomeMail,
        R1999TemplateKey.StartChapter,
        R1999TemplateKey.Chapter3Text,
        R1999TemplateKey.SelectTargetChapter4,
        R1999TemplateKey.AttackCard,
        R1999TemplateKey.Chapter5Text,
        R1999TemplateKey.ExitButton,
        R1999TemplateKey.SummonX1Text,
        R1999TemplateKey.LackUnilogText,
        R1999TemplateKey.DontHaveEnoughText,
        R1999TemplateKey.CharacterLevelText,
    ];

    private readonly Enum[] RenewAccountTemplates = [
        R1999TemplateKey.SummonX1Text,
        R1999TemplateKey.HomeMail,
        R1999TemplateKey.SettingButton,
        R1999TemplateKey.LogOutExitBtn,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.LoginAnotherAccBtn,
        // R1999TemplateKey.RegisterBtn,
        R1999TemplateKey.RegisterAccHeader,
        R1999TemplateKey.SentCodeBtn,
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

        var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
        var checkTemplates = CheckTemplatesAll;
        if (gameInstance == null)
        {
            return CoreAction.Empty;
        }

        if (gameInstance.JobReRollState.ReRollStatus == R1999ReRollStatus.SaveResultOk)
        {
            checkTemplates = RenewAccountTemplates;
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