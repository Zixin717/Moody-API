namespace Moody_backend.Models
{
    public class Diary
    {   /* mockEntries.js 資料結構 */
        public int Id { get; set; }
        public string Date { get; set; }     // 預期結果 -> "2026 / 04 / 20"
        public string Title { get; set; }
        public string Mood { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }    // 預期結果 -> "#D4E2A5"
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 建立時間


    }
}
