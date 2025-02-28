using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NDBotUI.Modules.Core.Db;
using NDBotUI.Modules.Core.Model;

namespace NDBotUI.Modules.Core.Repository;

public class ConfigRepository(ApplicationDbContext context)
{
    /// <summary>
    /// Lấy danh sách tất cả cấu hình
    /// </summary>
    public async Task<List<Config>> GetListAsync()
    {
        return await context.Configs.ToListAsync();
    }

    /// <summary>
    /// Lấy cấu hình theo tên
    /// </summary>
    public async Task<Config?> GetByNameAsync(string name)
    {
        return await context.Configs.FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <summary>
    /// Tạo mới một cấu hình
    /// </summary>
    public async Task<bool> CreateNewAsync(string name, string? value)
    {
        if (await context.Configs.AnyAsync(c => c.Name == name))
        {
            return false; // Đã tồn tại config này
        }

        var config = new Config { Name = name, Value = value };
        context.Configs.Add(config);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Cập nhật hoặc tạo mới config nếu chưa tồn tại
    /// </summary>
    public async Task UpdateOrCreateByNameAsync(string name, string? value)
    {
        var config = await context.Configs.FirstOrDefaultAsync(c => c.Name == name);

        if (config == null)
        {
            config = new Config { Name = name, Value = value };
            context.Configs.Add(config);
        }
        else
        {
            config.Value = value;
        }

        await context.SaveChangesAsync();
    }
}
