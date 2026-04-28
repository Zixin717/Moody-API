using BCrypt.Net;             // 引入加密套件
using Microsoft.AspNetCore.Mvc;
using Moody_backend.Data;     // 引入 Data -> 才能讀懂 AppDbContext
using Moody_backend.Models;
using System.Threading.Tasks; // 為了使用 async Task


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
            Console.WriteLine($"建立時間為 {newUser.CreatedAt}，資料庫已自動設置真實 ID: {newUser.Id}。");
            Console.WriteLine($"========================\n");

            /* 1-6 回傳結果給前端 */
            return Ok(new
            {
                message = "註冊成功！資料已寫入資料庫。",
                userId = newUser.Id,         // 把真實的 ID 傳給前端看看
                userEmail = newUser.Email,
                hashedPw = newUser.Password, // 測試用，正式上線記得拿掉
                buildTime = newUser.CreatedAt
            });
        }

        /* ===== Block 2: 登入 ===== */
        // 真實登入 API
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            /* 2-1 去資料庫找找看有沒有這個 Email */
            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);

            /* 2-2 找不到 Email，或者 驗證密碼 (Verify) 失敗 -> 回傳 401 未授權 */
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("信箱或密碼錯誤！");
            }

            /* 2-3 登入成功 -> 回傳使用者的非機密資料給前端 (不要把密碼傳回去) */
            return Ok(new
            {
                message = "登入成功！",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    nickname = user.Nickname,
                    birthday = user.birthday
                }
            });
        }
    }

    /* ===== Block 2: 登入 ===== */
    // 專門用來接登入資料的小類別 (放在 UserController 外面，同一個檔案即可)
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}