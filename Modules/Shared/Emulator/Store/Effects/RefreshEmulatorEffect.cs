using System;
using System.Threading.Tasks;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Shared.Emulator.Services;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Shared.Emulator.Store.Effects;

public class RefreshEmulatorEffect : EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [EmulatorAction.EmulatorRefresh,];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        Logger.Info(">>Process RefreshEmulatorEffect");
        try
        {
            await Task.Delay(0);
            var emulatorManager = EmulatorManager.Instance;
            emulatorManager.RefreshDevices(false, false);
            return EmulatorAction.EmulatorConnectSuccessAction.Create(emulatorManager.EmulatorConnections);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while processing init ADB");
            return EmulatorAction.EmulatorConnectError.Create();
        }
    }
}