using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("DiaryMoodSelection")]
    public class DiaryMoodSelection
    {
        public long DiaryId { get; set; }
        public string MoodId { get; set; } = string.Empty;
        public Mood Mood { get; set; } = null!; // 導航 -> 連 Mood 表拿心情資料
    }
}