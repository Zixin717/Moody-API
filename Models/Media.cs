using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("DiaryMedia")]
    public class DiaryMedia
    {
        [Key]
        [Column("MediaId")]
        public string MediaId { get; set; } = string.Empty;

        [Column("DiaryId")]
        public long DiaryId { get; set; }

        [Column("MediaType")]
        public string MediaType { get; set; } = "image";

        [Column("FileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
