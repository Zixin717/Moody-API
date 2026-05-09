// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Moody_backend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Data
{
    // DbContext = 資料庫管理員
    // 負責把 C# 的 User 物件翻譯成 SQL 指令
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet<User> = 代表資料庫裡的 Users 資料表
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Diary> Diaries { get; set; }
        public DbSet<DiaryNormal> DiaryNormals { get; set; }
        public DbSet<DiaryMood> DiaryMoods { get; set; }
        public DbSet<DiaryTag> DiaryTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<DiaryMoodSelection> DiaryMoodSelections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<Diary>().ToTable("Diary");
            modelBuilder.Entity<DiaryNormal>().ToTable("DiaryNormal");
            modelBuilder.Entity<DiaryMood>().ToTable("DiaryMood");

            // Diary 和 DiaryNormal 是一對一的關係，DiaryId 同時是 DiaryNormal 的主鍵和外鍵。
            modelBuilder.Entity<Diary>()
                .HasOne(d => d.Normal)
                .WithOne()
                .HasForeignKey<DiaryNormal>(n => n.DiaryId);

            modelBuilder.Entity<Diary>()
                .HasOne(d => d.MoodDetail)
                .WithOne()
                .HasForeignKey<DiaryMood>(m => m.DiaryId);

            // DiaryTag -> 複合主鍵 DiaryId + TagId 兩個欄位合在一起當主鍵
            modelBuilder.Entity<DiaryTag>()
                .HasKey(dt => new { dt.DiaryId, dt.TagId });

            // EF -> DiaryTag 連 Diary 
            modelBuilder.Entity<DiaryTag>()
                .HasOne<Diary>()
                .WithMany(d => d.DiaryTags)
                .HasForeignKey(dt => dt.DiaryId);

            // EF -> DiaryTag 連 Tag
            modelBuilder.Entity<DiaryTag>()
                .HasOne(dt => dt.Tag)
                .WithMany()
                .HasForeignKey(dt => dt.TagId);

            modelBuilder.Entity<DiaryMoodSelection>()
                .HasKey(ms => new { ms.DiaryId, ms.MoodId }); // 複合主鍵

            modelBuilder.Entity<DiaryMoodSelection>()
                .HasOne(ms => ms.Mood)
                .WithMany()
                .HasForeignKey(ms => ms.MoodId);

            modelBuilder.Entity<DiaryMoodSelection>()
                .HasOne<Diary>()
                .WithMany(d => d.MoodSelections)
                .HasForeignKey(ms => ms.DiaryId);

        }

        
    }
}

/*
 
 [Column(TypeName = "date")]
        public DateTime? birthday { get; set; }
 
 */