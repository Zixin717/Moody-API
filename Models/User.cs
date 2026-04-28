namespace Moody_backend.Models
{
    public class User
    {
        public int    Id       { get; set; } // Primary Key -> 不使用 E-mail，主鍵傳來傳去會爆炸。
        public string Email    { get; set; } // 註冊時填寫的 E-mail
        public string Password { get; set; } // 註冊填的密碼
        public string Phone    { get; set; }
        public string Nickname { get; set; }
        public string birthday { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 建立時間


    }
}
