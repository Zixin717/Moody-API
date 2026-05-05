namespace Moody_backend.Models
{
    public class User
    {
        public int    UserId { get; set; } // Primary Key -> 不使用 E-mail，主鍵傳來傳去會爆炸。
        public string Email    { get; set; } // 註冊時填寫的 E-mail
        public string Password { get; set; } // 註冊填的密碼
        public string Phone    { get; set; }
        public string Nickname { get; set; }
        public string birthday { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 建立時間
        public string? ResetCode { get; set; }             // 驗證碼
        public DateTime? ResetCodeExpiration { get; set; } // 驗證碼保留時間
        public bool IsNotificationEnabled { get; set; } = true; // 預設開啟
        public string Theme { get; set; } = "Beige";            // 主題


    }
}
