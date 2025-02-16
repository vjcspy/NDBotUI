using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.Emulator.Typing;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.MementoMori.Store.Effects.ReRollEffects;

public class SpamClickWhenCouldNotDetect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [MoriAction.CouldNotDetectMoriScreen,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        if (action.Payload is not BaseActionPayload baseActionPayload)
        {
            return CoreAction.Empty;
        }

        Logger.Info("Spam click when could not detect template");
        var emulatorConnection = EmulatorManager.Instance.GetConnection(baseActionPayload.EmulatorId);

        if (emulatorConnection == null)
        {
            return CoreAction.Empty;
        }

        await emulatorConnection.ClickPPointAsync(new PPoint(93.2f, 93.6f));
        await Task.Delay(250);
        // await emulatorConnection.ClickPPointAsync(new PPoint(95.6f, 6.8f));


        return CoreAction.Empty;
    }
}