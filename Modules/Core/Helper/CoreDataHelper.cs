using System.Threading.Tasks;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Repository;

namespace NDBotUI.Modules.Core.Helper;

public class CoreDataHelper
{
    public static async Task InitConfigData()
    {
        using var context = new ApplicationDbContext();
        var repository = new ConfigRepository(context);
        await repository.CreateNewAsync("r1999_email", "dinhkhoi.le.game@gmail.com");
    }
}