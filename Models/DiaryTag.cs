using Azure;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moody_backend.Models
{
    [Table("DiaryTag")]
    public class DiaryTag
    {  
            public long DiaryId { get; set; }
            public string TagId { get; set; }
            public Tag Tag { get; set; } // 導航：讓 EF 知道怎麼連到 Tag 表 
    }
}
