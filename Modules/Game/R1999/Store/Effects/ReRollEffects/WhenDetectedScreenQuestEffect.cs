using System;
using System.Linq;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects.ReRollEffects;

public class WhenDetectedScreenQuestEffect : EffectBase
{
    private readonly Enum[] _clickOnTemplateKeys =
    [
        R1999TemplateKey.SkipMovieBtn1,
        R1999TemplateKey.ConfirmBtn,
        R1999TemplateKey.Story1Text,
        R1999TemplateKey.StartChapter,
        R1999TemplateKey.Chapter3Text,
        R1999TemplateKey.AttackCard,
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
        Logger.Info(">>Process WhenDetectedScreenQuestEffect");
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
            case R1999TemplateKey.SignHere:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(55.7f, 36.5f));
                await Task.Delay(200);
                var gameInstance = AppStore.Instance.R1999Store.State.GetGameInstance(baseActionPayload.EmulatorId);
                await emulatorConnection.SendTextAsync($"chichi{gameInstance?.JobReRollState?.Ordinal}");
                await Task.Delay(200);
                //confirm
                await emulatorConnection.ClickPPointAsync(new PPoint(56.6f, 67.2f));
                await Task.Delay(200);
                await emulatorConnection.ClickPPointAsync(new PPoint(56.6f, 67.2f));

                await Task.Delay(200);
                await emulatorConnection.ClickPPointAsync(new PPoint(63.3f, 74.4f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SelectSkill1Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(55.7f, 84.5f));
                await Task.Delay(200);
                await emulatorConnection.ClickPPointAsync(new PPoint(65.4f, 86.1f));
                await Task.Delay(200);
                isClicked = true;
                break;
            }
            case R1999TemplateKey.HoldSkill2Text:
            {
                await emulatorConnection.SwipePPointAsync(new PPoint(65.7f, 91f), new PPoint(65.7f, 91f));
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(65.7f, 91f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.LinkSkillNeighboring:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(66.7f, 83.4f));
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(75.5f, 85.7f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.GuideMeUltimate:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(84.7f, 86.4f));
                isClicked = true;
                break;
            }
            case R1999TemplateKey.SkillUltimateText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(75.2f, 85.4f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.ReturnStoryText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.3f, 6.3f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.RitualTimeKeeperText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(75.1f, 85.4f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.FullyPreparedText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(94.1f, 86.5f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SkillTimekeeperText:
            {
                // swipe
                await emulatorConnection.SwipePPointAsync(new PPoint(57.1f, 84.4f), new PPoint(84f, 86.9f));
                await Task.Delay(8000);
                // skill
                await emulatorConnection.ClickPPointAsync(new PPoint(94.3f, 84.8f));
                await Task.Delay(10000);

                // swipe
                await emulatorConnection.SwipePPointAsync(new PPoint(66.7f, 86.8f), new PPoint(93.1f, 85.0f));
                await Task.Delay(2000);
                // skill
                await emulatorConnection.ClickPPointAsync(new PPoint(94.3f, 84.8f));
                await Task.Delay(10000);

                // skill 1
                await emulatorConnection.ClickPPointAsync(new PPoint(65.9f, 84.4f));
                // skill 2
                await emulatorConnection.ClickPPointAsync(new PPoint(75.9f, 85.8f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.Chapter1Button:
            {
                var pPoint = emulatorConnection.ToPPoint(detectTemplatePoint.Point);
                if (pPoint != null)
                {
                    await emulatorConnection.ClickPPointAsync(pPoint with { X = pPoint.X - 13, Y = pPoint.Y, });
                    isClicked = true;
                }

                break;
            }

            case R1999TemplateKey.StartLoss8Button:
            case R1999TemplateKey.StartLoss8Button2:
            {
                var pPoint = emulatorConnection.ToPPoint(detectTemplatePoint.Point);
                if (pPoint != null)
                {
                    await emulatorConnection.ClickPPointAsync(pPoint with { X = pPoint.X + 5, Y = pPoint.Y + 3, });
                    isClicked = true;
                }

                break;
            }
            case R1999TemplateKey.AccelerateBattleText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(97.2f, 5.7f));
                isClicked = true;
                break;
            }
            case R1999TemplateKey.ChooseEnemyText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(15.2f, 54.7f));
                await Task.Delay(1000);
                await emulatorConnection.ClickPPointAsync(new PPoint(85.3f, 85.1f));
                await Task.Delay(7000);
                await emulatorConnection.ClickPPointAsync(new PPoint(85.3f, 85.1f));
                await Task.Delay(7000);
                await emulatorConnection.ClickPPointAsync(new PPoint(85.3f, 85.1f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SummonText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(81.0f, 68.9f));
                await Task.Delay(1000);
                isClicked = true;
                break;
            }
            case R1999TemplateKey.SummonWheel:
            {
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(100);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(300);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(300);
                await emulatorConnection.SwipePPointAsync(new PPoint(59.1f, 36.0f), new PPoint(60.1f, 56.9f),200);
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(49.9f, 73.9f));
                await Task.Delay(10000);
                await emulatorConnection.ClickPPointAsync(new PPoint(4f, 5.3f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SomeoneFamiliarText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(49.9f, 74.1f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SummonX1Text:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(11.5f, 5.8f));
                await Task.Delay(100);
                await emulatorConnection.ClickPPointAsync(new PPoint(3.7f, 6.0f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SonettoGuideLv:
            {
                //click level
                await emulatorConnection.ClickPPointAsync(new PPoint(68.4f, 20.2f));
                await Task.Delay(4000);
                //select lv3
                await emulatorConnection.ClickPPointAsync(new PPoint(88.5f, 51.3f));
                await Task.Delay(3000);
                await emulatorConnection.ClickPPointAsync(new PPoint(80.2f, 70.0f));
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(80.8f, 82.0f));
                await Task.Delay(1000);
                // return story
                await emulatorConnection.ClickPPointAsync(new PPoint(11.0f, 6.0f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.CheckCrewText:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(88.7f, 54.7f));
                await Task.Delay(1500);

                // select sonetto
                await emulatorConnection.ClickPPointAsync(new PPoint(15.1f, 35.1f));
                await Task.Delay(1500);

                isClicked = true;
                break;
            }

            case R1999TemplateKey.HomeMail:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(83.9f, 40.3f));
                isClicked = true;
                break;
            }

            case R1999TemplateKey.SelectTargetChapter4:
            {
                await emulatorConnection.ClickPPointAsync(new PPoint(12.7f, 64.7f));
                await Task.Delay(2000);
                await emulatorConnection.ClickPPointAsync(new PPoint(65.3f, 84.3f));
                await Task.Delay(1500);
                await emulatorConnection.ClickPPointAsync(new PPoint(95.0f, 84.3f));
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

                return currentStatus < R1999ReRollStatus.FinishQuest;
            }
        }

        return false;
    }
}