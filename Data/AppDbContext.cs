// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Moody_backend.Models;

namespace Moody_backend.Data
{
    // DbContext = 資料庫管理員
    // 負責把 C# 的 User 物件翻譯成 SQL 指令
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet<User> = 代表資料庫裡的 Users 資料表
        public DbSet<User> Users { get; set; }
    }
}