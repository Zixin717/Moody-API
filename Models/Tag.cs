using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("Tag")]
    public class Tag
    {
            [Key]
            public string TagId { get; set; }
            public int? UserId { get; set; }
            public string TagName { get; set; }
            public string TagType { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }    
    }
}
