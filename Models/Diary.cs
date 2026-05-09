using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    // 對應 dbo.Diary（日記公版）
    [Table("Diary")]
    public class Diary
    {
        [Key]
        public long DiaryId { get; set; }
        public int UserId { get; set; }
        public string TemplateType { get; set; }   // "normal" or "mood"
        public string? PreviewText { get; set; }
        public DateOnly DiaryDate { get; set; }    // 星球對應的日期
        public TimeOnly DiaryTime { get; set; }
        public string? WeatherType { get; set; }
        public string Visibility { get; set; } = "private";
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DiaryNormal? Normal { get; set; }   // 關聯：日記 Normal 內容 -> 用於主頁面懸停卡片顯示
        public DiaryMood? MoodDetail { get; set; } // 關聯：日記 Mood 內容 -> 用於主頁面懸停卡片顯示
        public ICollection<DiaryTag> DiaryTags { get; set; } = new List<DiaryTag>(); // 關聯 -> 右側欄位日記 Tag
        public ICollection<DiaryMoodSelection> MoodSelections { get; set; } = new List<DiaryMoodSelection>(); // 關聯 -> 右側欄位心情顯示



    }

    // 對應 dbo.DiaryNormal（一般日記）
    [Table("DiaryNormal")]
    public class DiaryNormal
    {
        [Key]
        public long DiaryId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
    }

    // 對應 dbo.DiaryMood（心情日記）
    [Table("DiaryMood")]
    public class DiaryMood
    {
        [Key]
        public long DiaryId { get; set; }
        public byte? EnergyValue { get; set; }
        public byte? StressValue { get; set; }
        public byte? SleepValue { get; set; }
        public string? EventNote { get; set; }
        public string? ThoughtNote { get; set; }
        public string? NeedNote { get; set; }
    }

}
