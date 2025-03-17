using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.Ete.Store.Effects.ReRollEffects;

public class ReRollStartEffect : EffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        EteTemplateKey.StartCloseDialogButton,
        EteTemplateKey.LoginAgreementText,
        EteTemplateKey.ClickStrike,
        EteTemplateKey.GuideNoPrblText,
    ];

    protected override bool IsParallel()
    {
        return false;
    }

    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EteAction.DetectScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process ReRollStartEffect");
        if (action.Payload is not BaseActionPayload baseActionPayload
            || baseActionPayload.Data is not DetectTemplatePoint detectTemplatePoint)
        {
            return CoreAction.Empty;
        }

        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection is null)
        {
            return CoreAction.Empty;
        }

        var isClicked = false;

        switch (detectTemplatePoint.TemplateKey)
        {
            case EteTemplateKey.LoginAgreementText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(42.7f, 87.3f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.GraphicQualityText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(50.7f, 88.7f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.SkipButton:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(92.3f, 8.8f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.GuideJoystick:
            {
                await emulatorConnection.SwipePPointAsync(new PPoint(15.4f, 74.1f),new PPoint(17.4f, 71.1f));
                isClicked = true;
                break;
            }
            case EteTemplateKey.GuideNormalAttk:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(91.6f, 76.3f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.GuideIntrinsicSkill:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(72.7f, 85.3f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.GuideEnterName:
            {
                await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                await Task.Delay(550);

                await emulatorConnection.SendTextAsync("ngocdiep");
                await Task.Delay(550);
                await emulatorConnection.ClickOnPointAsync(detectTemplatePoint.Point);
                await Task.Delay(250);
                await emulatorConnection.ClickPPointAsync(new PPoint(50.2f, 62.7f)); // ok
                isClicked = true;
                break;
            }

            case EteTemplateKey.InBattleCharIcon1:
            case EteTemplateKey.InBattleCharIcon11:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(91.2f, 78.2f));
                await Task.Delay(100);
                await emulatorConnection.ClickPPointAsync(new PPoint(72.6f, 85.8f));
                await Task.Delay(100);
                await emulatorConnection.ClickPPointAsync(new PPoint(64.2f, 86.2f));

                // Run
                await Task.Delay(100);
                await emulatorConnection.ClickPPointAsync(new PPoint(82.8f, 85.6f));

                // Ultimate
                await Task.Delay(100);
                await emulatorConnection.ClickPPointAsync(new PPoint(87.4f, 19.9f));
                isClicked = true;
                break;
            }

            case EteTemplateKey.GuideEnemy2Assault:
            case EteTemplateKey.GuideEnemy2Assault2:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(8.8f, 25.1f));
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
}