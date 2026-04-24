using Microsoft.AspNetCore.Mvc;
using Moody_backend.Models;

namespace Moody_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] User newUser)
        {
            // 1. 這裡會收到前端 React 傳過來的 formData

            // 2. 檢查資料有沒有填寫 (簡單示範)
            if (string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            // ※ 真實開發注意：必須將 newUser.Password 進行 Hash 加密 (如 BCrypt)，絕對不能明碼存入資料庫！

            // 3. TODO: 把 newUser 存進 SQL Server 資料庫
            // 目前我們先假設存檔成功，直接回傳成功訊息

            return Ok(new { message = "註冊成功！", user = newUser.Email });
        }
    }
}