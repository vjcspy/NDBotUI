using System.Threading.Tasks;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Helper;
using NDBotUI.Modules.Core.Repository;
using NDBotUI.Modules.Game.AutoCore.Store;
using NDBotUI.Modules.Game.R1999.Helper;
using NDBotUI.Modules.Shared.EventManager;

namespace NDBotUI.Modules.Game.R1999.Store.Effects;

public class InitR1999Effect: EffectBase
{
    protected override IEventActionFactory[] GetAllowEventActions()
    {
        return [R1999Action.InitR1999];
    }

    protected override async Task<EventAction> Process(EventAction action)
    {
        await Task.Delay(0);
        // Init template for scanning image
        R1999ScreenDetectorDataHelper.GetInstance().LoadData();

        await using var context = new ApplicationDbContext();
        var configRepository = new ConfigRepository(context);
        var emailConfig = await configRepository.GetByNameAsync("r1999_email");
        if (emailConfig != null && !string.IsNullOrEmpty(emailConfig.Value))
        {
            R1999DataHelper.AccountEmail = emailConfig.Value;
        }
        return CoreAction.Empty;
    }
}