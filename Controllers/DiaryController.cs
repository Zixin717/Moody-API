using Microsoft.AspNetCore.Mvc;
using Moody_backend.Models; // 引入日記模型


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moody_backend.Controllers
{
    [Route("api/[controller]")] // 設定路由為 api/Diary
    [ApiController]
    public class DiaryController : ControllerBase
    {
        // GET: api/<DiaryController>
        //      前端發送 get 請求時，執行這段代碼。
        // 當前端發送 GET 請求時，會執行這個方法
        [HttpGet]
        public IActionResult GetDiaries()
        {
            // C# 版本的 mockEntries -> 先寫死兩筆假資料測試，非正式實現。（勿動）
            var diaries = new List<Diary>
            {
                new Diary {
                    Id = 1,
                    Date = "2026 / 04 / 20",
                    Title = "愉快的一天",
                    Mood = "開心",
                    Content = "今天去公園散步，看到很多花。",
                    Color = "#D4E2A5"
                },
                new Diary {
                    Id = 2,
                    Date = "2026 / 04 / 21",
                    Title = "有點焦慮",
                    Mood = "焦慮",
                    Content = "專案快來不及了，壓力有點大。",
                    Color = "#fca5a5"
                }
            };

            // 把資料包裝成成功的 HTTP 回應 (200 OK) 送出去
            return Ok(diaries);
        }

        // GET api/<DiaryController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<DiaryController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT api/<DiaryController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<DiaryController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
