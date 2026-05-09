using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("Mood")]
    public class Mood
    {
        [Key]
        public string MoodId { get; set; } = string.Empty;
        public string MoodName { get; set; } = string.Empty;
        public string MoodEmoji { get; set; } = string.Empty;
        public bool IsPositive { get; set; }
        public bool IsHighEnergy { get; set; }
    }
}
