using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NDBotUI.Modules.Game.R1999.Db;

namespace NDBotUI.Modules.Game.R1999.Service;

public class R1999AccountService(DbContext context)
{
    private static readonly Lock Lock = new Lock(); // Khóa đồng bộ cho method

    public R1999Account CreateNewAccount(string email, AccountStatus status = AccountStatus.Open)
    {
        // Áp dụng khóa đồng bộ để đảm bảo tính tuần tự
        lock (Lock)
        {
            // Bước 1: Lấy Ordinal cao nhất hiện tại trong cơ sở dữ liệu
            var latestAccount = context
                .Set<R1999Account>()
                .OrderByDescending(a => a.Ordinal)
                .FirstOrDefault();

            int newOrdinal = (latestAccount?.Ordinal ?? 0) + 1; // Nếu không có dữ liệu, Ordinal sẽ là 1

            // Bước 2: Tạo đối tượng mới với Ordinal mới
            var newAccount = new R1999Account
            {
                Email = email,
                AccountStatus = status,
                Ordinal = newOrdinal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Bước 3: Thêm dòng mới vào cơ sở dữ liệu và lưu thay đổi
            context
                .Set<R1999Account>()
                .Add(newAccount);
            context.SaveChanges();

            // Bước 4: Trả về dòng mới
            return newAccount;
        }
    }
}