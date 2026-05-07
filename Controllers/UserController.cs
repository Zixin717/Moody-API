using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // SaveChangesAsync

using System.Net;
using System.Net.Mail;        // 寄信套件
using System.Threading.Tasks; // async Task

using BCrypt.Net;             // 加密套件

using Moody_backend.Data;     // 引入 Data -> 才能讀懂 AppDbContext
using Moody_backend.Models;




namespace Moody_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /* ===== Block 0 ========== */
        // 鑰匙 -> 透過依賴注入 (DI) 拿到資料庫的控制權
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        /* ===== Block 1: 註冊 ===== */
        // ※ 這裡加上 async Task -> 存取資料庫需要一點時間，讓它非同步執行才不會卡住。
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User newUser)
        {
            /* 1-1 檢查資料是否填寫 */
            if (string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            /* 1-2 檢查 Email 是否已被註冊 (防止重複帳號) */
            // 去 Users 表 裡面找，是否有用戶的 Email 跟現在要註冊的一樣。
            bool emailExists = _db.Users.Any(u => u.Email == newUser.Email);
            if (emailExists)
            {
                return Conflict("此 Email 已被註冊！"); // 409 Conflict 狀態碼
            }

            /* 1-3 密碼加密 */
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            newUser.Password = hashedPassword; // 把原本的明碼替換成加密後的密碼

            /* 1-4 寫入資料庫 */
            _db.Users.Add(newUser);          // 1. 先把這筆新資料放到路線上
            await _db.SaveChangesAsync();    // 2. 真正執行 SQL INSERT

            /* 1-5 終端機測試紀錄 */
            Console.WriteLine($"\n=== 收到新用戶註冊！ ===");
            Console.WriteLine($"Email: {newUser.Email}");
            Console.WriteLine($"原始密碼已銷毀，加密後密碼: {newUser.Password}");

            // 經過 SaveChangesAsync 之後，資料庫配發了真實的ID。
            Console.WriteLine($"建立時間為 {newUser.CreatedAt}，資料庫已自動設置真實 ID: {newUser.UserId}。");
            Console.WriteLine($"========================\n");

            /* 1-6 回傳結果給前端 */
            return Ok(new
            {
                message = "註冊成功！資料已寫入資料庫。",
                userId = newUser.UserId,         // 把真實的 ID 傳給前端看看
                userEmail = newUser.Email,
                hashedPw = newUser.Password, // 測試用，正式上線記得拿掉
                buildTime = newUser.CreatedAt
            });
        }

        /* ===== Block 2: 登入 ===== */
        // 登入 API
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            /* 2-1 資料庫撈 Email */
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);

            /* 2-2 撈不到 Email || 帳號已刪除      || 驗證密碼 (Verify) 失敗 -> 回傳 401 未授權 */
            if (user == null   || user.IsDeleted || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("信箱或密碼錯誤！");
            }

            /* 2-3 登入成功 -> 回傳使用者的非機密資料給前端 (不要把密碼傳回去) */
            return Ok(new
            {
                message = "登入成功！",
                user = new
                {
                    id = user.UserId,
                    email = user.Email,
                    nickname = user.Nickname,
                    birthday = user.birthday,
                    phone = user.Phone
                }
            });
        }


        /* ===== Block 3: 更新個人資料 ===== */
        // 用 [HttpPut("{id}")] 代表要更新某個特定 ID 的用戶
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            // 3-1 去資料庫找這個用戶
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("找不到該用戶！");
            }

            // 3-2 將前端傳來的新資料，覆蓋掉資料庫裡的舊資料。
            user.Nickname = request.Nickname;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.birthday = request.Birthday;

            // 3-3 儲存變更 -> 這行會自動產生 SQL 的 UPDATE 指令
            await _db.SaveChangesAsync();

            // 3-4 回傳更新後的最新資料給前端
            return Ok(new
            {
                message = "資料更新成功！",
                user = new
                {
                    id = user.UserId,
                    email = user.Email,
                    nickname = user.Nickname,
                    phone = user.Phone,
                    birthday = user.birthday
                }
            });
        }

        /* ===== Block 4: 驗證舊密碼 (Step 1) ===== */
        [HttpPost("verify-password")]
        public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest request)
        {
            // 1. 從資料庫找到該用戶
            var user = _db.Users.FirstOrDefault(u => u.UserId == request.UserId);
            if (user == null)
            {
                return NotFound("找不到該用戶");
            }

            // 2. 驗證密碼是否正確
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordCorrect)
            {
                return Unauthorized("舊密碼錯誤");
            }

            return Ok(new { message = "驗證成功，允許修改密碼" });
        }

        /* ===== Block 4: 設定新密碼 (Step 2) ===== */
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // 1. 從資料庫找到該用戶
            var user = _db.Users.FirstOrDefault(u => u.UserId == request.UserId);
            if (user == null)
            {
                return NotFound("找不到該用戶");
            }

            // 2. 雙重保險，再次驗證舊密碼 -> 防駭客繞過 Step 1 直接打這支 API
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                return Unauthorized("舊密碼驗證失敗，無法修改");
            }

            // 3. 將新密碼加密
            string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 4. 覆蓋資料庫裡的舊密碼
            user.Password = hashedNewPassword;
            await _db.SaveChangesAsync(); // 存檔！

            return Ok(new { message = "密碼修改成功" });
        }


        /* ===== Block 5: 忘記密碼 - 寄送驗證信 ===== */
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // 1. 確認信箱是否存在
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("找不到此信箱註冊的帳號");
            }

            // 2. 產生 4 位數隨機碼與過期時間 (10 分鐘)
            Random rnd = new Random();
            string code = rnd.Next(1000, 9999).ToString();
            user.ResetCode = code;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync(); // 存入資料庫

            // 3. 設定郵差與信件內容
            try
            {
                // 測試階段 -> 換成申請好的 Gmail 與 16 碼應用程式密碼
                string systemEmail = "sepu5ma836@gmail.com";
                string systemAppPassword = "yczbvwptiojolkwm";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(systemEmail, "Moody 行動中心");
                mail.To.Add(request.Email);
                mail.Subject = "Moody 密碼重設驗證碼";
                mail.Body = $"<h3>您好，{user.Nickname}：</h3><p>您的密碼重設驗證碼為：<strong style='font-size:24px;color:#A1A34E;'>{code}</strong></p><p>請在 10 分鐘內輸入此驗證碼。若非本人操作，請忽略此信件。</p>";
                mail.IsBodyHtml = true;

                // 4. 呼叫 Google SMTP 伺服器幫忙送信
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(systemEmail, systemAppPassword);
                    smtp.EnableSsl = true;          // 必須開啟加密
                    await smtp.SendMailAsync(mail); // 發射信件！
                }

                return Ok(new { message = "驗證碼已寄出，請至信箱收取" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("寄信失敗: " + ex.Message);
                return StatusCode(500, "寄信系統發生錯誤，請稍後再試");
            }
        }

        /* ===== Block 7: 忘記密碼 - 核對驗證碼 ===== */
        [HttpPost("verify-reset-code")]
        public IActionResult VerifyResetCode([FromBody] VerifyCodeRequest request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return NotFound("找不到此用戶");

            // 檢查驗證碼對不對，以及有沒有超時
            if (user.ResetCode != request.Code)
            {
                return BadRequest("驗證碼錯誤");
            }
            if (DateTime.UtcNow > user.ResetCodeExpiration)
            {
                return BadRequest("驗證碼已過期，請重新發送");
            }

            return Ok(new { message = "驗證成功，允許重設密碼" });
        }

        /* ===== Block 8: 忘記密碼 - 設定新密碼 ===== */
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return NotFound("找不到此用戶");

            // 雙重保險：防駭客直接打這支 API，再檢查一次驗證碼
            if (user.ResetCode != request.Code || DateTime.UtcNow > user.ResetCodeExpiration)
            {
                return BadRequest("驗證無效或已過期");
            }

            // 加密新密碼
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 安全機制：密碼改完後，把驗證碼清空，避免被重複使用
            user.ResetCode = null;
            user.ResetCodeExpiration = null;

            await _db.SaveChangesAsync();
            return Ok(new { message = "密碼重設成功，請使用新密碼登入" });
        }

        /* ===== Block 9: 更新系統設定 -> 主題切換、通知設定 ===== */
        [HttpPut("update-settings/{userId}")]
        public async Task<IActionResult> UpdateSettings(int userId, [FromBody] UpdateSettingsRequest request)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("找不到該用戶！");

            // 只更新設定欄位
            user.IsNotificationEnabled = request.IsNotificationEnabled;
            user.Theme = request.Theme;

            await _db.SaveChangesAsync();

            return Ok(new { message = "設定已自動儲存", user });
        }

        /* ===== Block 10: 帳戶刪除（隱藏狀態） ===== */
        [HttpPut("delete/{userId}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return NotFound("找不到該用戶！");
            }
            // 改一個狀態欄位，讓前端知道這個帳號已被刪除，不能再登入。
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "帳戶已刪除（隱藏狀態）" });
        }
    }
    /* ===== Request：登入 ===== */
    // 專門用來接登入資料的小類別 (放在 UserController 外面，同一個檔案即可)
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /* ===== Request：更新個人資料 ===== */
    // 用來接更新資料的類別
    public class UpdateUserRequest
    {
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
    }

    /* ===== Request：改密碼 ===== */
    public class VerifyPasswordRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    /* ===== Request：忘記密碼 ===== */
    public class ForgotPasswordRequest { public string Email { get; set; } }
    public class VerifyCodeRequest { public string Email { get; set; } public string Code { get; set; } }
    public class ResetPasswordRequest { public string Email { get; set; } public string Code { get; set; } public string NewPassword { get; set; } }

    /* ===== Request：更新系統設定 ===== */
    public class UpdateSettingsRequest
    {
        public bool IsNotificationEnabled { get; set; }
        public string Theme { get; set; }
    }

    

}