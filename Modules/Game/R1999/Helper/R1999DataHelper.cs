namespace NDBotUI.Modules.Game.R1999.Helper;

public class R1999DataHelper
{
    public static string GetAccountEmail(string? ordinal = null)
    {
        return ordinal != null ? $"dinhkhoi.le.sg+{ordinal}@gmail.com" : "dinhkhoi.le.sg@gmail.com";
    }
}