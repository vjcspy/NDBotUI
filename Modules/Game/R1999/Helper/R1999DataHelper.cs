using System;
using System.Threading.Tasks;

namespace NDBotUI.Modules.Game.R1999.Helper;

public class R1999DataHelper
{
    public static string AccountEmail = "dinhkhoi.le.game@gmail.com";

    public static string GetAccountEmail(string? ordinal = null)
    {
        if (string.IsNullOrEmpty(AccountEmail))
            throw new InvalidOperationException("AccountEmail is not set.");

        var parts = AccountEmail.Split('@');
        if (parts.Length != 2)
            throw new FormatException("Invalid email format in AccountEmail.");

        string username = parts[0];
        string domain = parts[1];

        return ordinal != null ? $"{username}+{ordinal}@{domain}" : AccountEmail;
    }
}
