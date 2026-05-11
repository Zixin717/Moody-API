using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("task")]
    public class HabitTask
    {
        [Key]
        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("rhythm_type")]
        public string RhythmType { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "Active";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
