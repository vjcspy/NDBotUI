using System;
using System.Drawing;
using System.Threading.Tasks;
using NDBotUI.Modules.Core.Extensions;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Store;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.MementoMori.Helper;
using NDBotUI.Modules.Shared.Emulator.Models;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class SpamClickWhenCouldNotDetect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.CouldNotDetectMoriScreen];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload) return CoreAction.Empty;

        Logger.Info("Spam click when could not detect template");
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection == null) return CoreAction.Empty;

        if (AppStore.Instance.MoriStore.State.GetGameInstance(baseActionPayload.EmulatorId) is {} gameInstance)
        {
            if (gameInstance.JobReRollState.CurrentLevel == 16)
            {
                var isInCharacterGrowth = await ScanTemplateImage(emulatorConnection,MoriTemplateKey.CharacterGrowthTabHeader);

                if (isInCharacterGrowth != null)
                {
                    Logger.Info("Current Chapter Lv16 -> Back to Quest");
                    await emulatorConnection.ClickPPointAsync(new PPoint(74.2f, 82.3f));
                    await Task.Delay(1250);
                    await emulatorConnection.ClickPPointAsync(new PPoint(74.2f, 82.3f));
                    await Task.Delay(1250);
                    await emulatorConnection.ClickPPointAsync(new PPoint(2.8f, 4.0f));
                    await Task.Delay(1250);
                    return CoreAction.Empty;
                }
            }
        }
        
        
        
        
        await emulatorConnection.ClickPPointAsync(new PPoint(93.2f, 93.6f));
        await Task.Delay(250);
        // await emulatorConnection.ClickPPointAsync(new PPoint(95.6f, 6.8f));
        
        

        return CoreAction.Empty;
    }
    
    private  async Task<Point?> ScanTemplateImage(EmulatorConnection emulatorConnection, MoriTemplateKey templateKey)
    {
        var screenshot = await emulatorConnection.TakeScreenshotAsync();
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