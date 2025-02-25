using System;

namespace NDBotUI.Modules.Game.R1999.Db;

public enum AccountStatus
{
    Open = 1,
    Processing = 2,
    Complete = 3,
}

public class R1999Account
{
    public int Id { get; set; }
    public string Email { get; set; }
    public int Ordinal { get; set; }
    public AccountStatus AccountStatus { get; set; }

    // Thêm trường CreatedAt và UpdatedAt
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}