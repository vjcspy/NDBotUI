using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Extensions;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Game.MementoMori.Store.State;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class EligibilityLevelCheck : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.DetectedMoriScreen, MoriAction.EligibilityLevelCheck];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info("Eligibility level check started");
        if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;
        try
        {
            var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

            if (emulatorConnection == null) return CoreAction.Empty;

            // Click outside to close battle
            await Task.Delay(1500);
            Logger.Info("Click outside to close battle");
            await emulatorConnection.ClickPPointAsync(new PPoint(93.2f, 8.7f));
            await Task.Delay(1500);
            await emulatorConnection.ClickPPointAsync(new PPoint(81.8f, 26.7f));
            await Task.Delay(2000);

            // click vao characters
            Logger.Info("Click into character growth tab");
            await emulatorConnection.ClickPPointAsync(new PPoint(20.8f, 93.9f));
            await Task.Delay(1500);

            // click char1
            Logger.Info("Click into character 1");
            await emulatorConnection.ClickPPointAsync(new PPoint(16.3f, 33.6f));
            await Task.Delay(1500);
            await LevelUpChar(emulatorConnection);

            // click char2
            Logger.Info("Click into character 2");
            await emulatorConnection.ClickPPointAsync(new PPoint(28.1f, 33.6f));
            await Task.Delay(1500);
            await LevelUpChar(emulatorConnection);

            // click char3
            Logger.Info("Click into character 3");
            await emulatorConnection.ClickPPointAsync(new PPoint(39.1f, 33.6f));
            await Task.Delay(1500);
            await LevelUpChar(emulatorConnection);

            // click char 4
            Logger.Info("Click into character 4");
            await emulatorConnection.ClickPPointAsync(new PPoint(50.1f, 33.6f));
            await Task.Delay(1500);
            await LevelUpChar(emulatorConnection);


            // Click về quest
            await emulatorConnection.ClickPPointAsync(new PPoint(44.4f, 95.1f));

            return MoriAction.EligibilityLevelPass.Create(baseActionPayload);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Eligibility Level check error");
            return MoriAction.EligibilityLevelCheckError.Create(baseActionPayload);
        }
    }

    private async Task<Point?> FindTemplate(MoriTemplateKey templateKey, Mat screenshotEmguMat)
    {
        await Task.Delay(0);
        // ensure o trong character growth
        if (TemplateImageDataHelper.TemplateImageData[templateKey].EmuCVMat is
            { } templateMat)
            return ImageFinderEmguCV.FindTemplateMatPoint(
                screenshotEmguMat,
                templateMat,
                debugKey: templateKey.ToString(),
                matchValue: 0.9
            );

        throw new Exception("Template image data is null");
    }

    private async Task LevelUpChar(EmulatorConnection emulatorConnection)
    {
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
        if (screenshot is null) throw new Exception("Screenshot is null");
        var screenshotEmguMat = screenshot.ToEmguMat();
        // ensure o trong character growth
        var point = await FindTemplate(MoriTemplateKey.CharacterGrowthTabHeader, screenshotEmguMat);

        if (point is null) throw new Exception("Not in Character Growth Tab");

        MoriTemplateKey[] lvToCheck =
        [
            MoriTemplateKey.CharacterLevelOneText, MoriTemplateKey.CharacterLevelTwoText,
            MoriTemplateKey.CharacterLevelThreeText, MoriTemplateKey.CharacterLevelFourText,
            MoriTemplateKey.CharacterLevelFiveText, MoriTemplateKey.CharacterLevelSixText
        ];

        var countLevelUp = 0;

        while (countLevelUp < 7)
        {
            var lv7Point = await FindTemplate(MoriTemplateKey.CharacterLevelSevenText, screenshotEmguMat);

            // char đã lv7
            if (lv7Point is not null)
            {
                Logger.Info("Character already lv 7");

                // equip all
                await emulatorConnection.ClickPPointAsync(new PPoint(36.3f, 82.4f));
                await Task.Delay(500);

                // back
                await emulatorConnection.ClickPPointAsync(new PPoint(3.1f, 3.5f));
                await Task.Delay(1500);
                return;
            }

            var mat = screenshotEmguMat;
            var tasks = lvToCheck.Select(moriTemplateKey => Observable
                .FromAsync(() => Task.Run(() => FindTemplate(moriTemplateKey, mat))));

            var result = await tasks.Merge()
                .Where(res => res != null)
                .ToList();

            if (result.Count == 0) throw new Exception("Unknown current level");

            // level up
            Logger.Info("Click Level Up");
            emulatorConnection.ClickPPoint(new PPoint(76.7f, 82.9f));
            countLevelUp += 1;
            await Task.Delay(1000);
            
            // refresh level screen
            screenshot = await emulatorConnection.TakeScreenshotAsync();
            if (screenshot is null) throw new Exception("Screenshot is null");

            screenshotEmguMat = screenshot.ToEmguMat();

            if (screenshotEmguMat.IsEmpty) throw new Exception("Screenshot Mat is empty");
        }
    }

    [Effect]
    public override RxEventHandler EffectHandler()
    {
        return upstream => upstream
            .OfAction(GetAllowEventActions())
            .Throttle(TimeSpan.FromMilliseconds(3000))
            .FilterBaseEligibility(GetForceEligible())
            .Where(action =>
            {
                var isValid = action.Payload is BaseActionPayload baseActionPayload &&
                              AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId)
                                  is { JobReRollState.ReRollStatus: ReRollStatus.EligibilityLevelCheck };

                return isValid;
            })
            .SelectMany(Process);
    }
}