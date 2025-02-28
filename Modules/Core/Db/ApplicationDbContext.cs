using System;
using Microsoft.EntityFrameworkCore;
using NDBotUI.Modules.Core.Model;
using NDBotUI.Modules.Game.R1999.Db;

namespace NDBotUI.Modules.Core.Db;

public class ApplicationDbContext : DbContext
{
    public DbSet<R1999Account> R1999Accounts { get; set; }
    public DbSet<Config> Configs { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=database.db");
    }

    /// <summary>
    /// Tự động chạy migrations và cập nhật database khi ứng dụng khởi động
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        Database.Migrate(); // Áp dụng migrations nếu cần
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình cột Status lưu dưới dạng chuỗi thay vì số nguyên
        modelBuilder
            .Entity<R1999Account>()
            .Property(u => u.AccountStatus)
            .HasConversion(
                v => v.ToString(), // Chuyển enum thành chuỗi khi lưu
                v => (AccountStatus)Enum.Parse(typeof(AccountStatus), v)
            ); // Chuyển chuỗi thành enum khi đọc

        modelBuilder.Entity<Config>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<R1999Account>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow; // Set CreatedAt khi thêm mới
                entry.Entity.UpdatedAt = DateTime.UtcNow; // Set UpdatedAt khi thêm mới
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow; // Cập nhật UpdatedAt khi sửa đổi
            }
        }

        return base.SaveChanges();
    }
}