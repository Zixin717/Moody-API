using Microsoft.AspNetCore.Mvc;
using Moody_backend.Data;
using Moody_backend.Models; // 引入日記模型
using Microsoft.EntityFrameworkCore;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moody_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiaryController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DiaryController(AppDbContext db)
        {
            _db = db;
        }

        // API 1：查某個月份所有有日記的日期
        // GET /api/diary/month?userId=1&year=2026&month=5
        // 前端 -> 用來決定哪些星球要點亮
        [HttpGet("month")]
        public async Task<IActionResult> GetMonthDiaries(int userId, int year, int month)
        {
            var diaries = await _db.Diaries
                .Where(d => d.UserId == userId
                         && d.DiaryDate.Year == year
                         && d.DiaryDate.Month == month
                         && d.Status != "deleted")
                .Select(d => new
                {
                    DiaryId = d.DiaryId,
                    DiaryDate = d.DiaryDate.ToString("yyyy-MM-dd"),
                    TemplateType = d.TemplateType,
                    PreviewText = d.PreviewText,
                })
                .ToListAsync();

            return Ok(diaries);
        }

        // API 2：查某一天的日記完整內容（懸停卡片 + 點擊進入詳細頁用）
        // GET /api/diary/by-date?userId=1&date=2026-05-01
        [HttpGet("by-date")]
        public async Task<IActionResult> GetDiaryByDate(int userId, string date)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return BadRequest("日期格式錯誤，請用 yyyy-MM-dd");

            var diary = await _db.Diaries
                .Include(d => d.Normal)
                .Include(d => d.MoodDetail)
                .FirstOrDefaultAsync(d => d.UserId == userId
                                       && d.DiaryDate == parsedDate
                                       && d.Status != "deleted");

            if (diary == null) return NotFound("這天沒有日記");

            return Ok(new
            {
                diaryId = diary.DiaryId,
                diaryDate = diary.DiaryDate.ToString("yyyy-MM-dd"),
                templateType = diary.TemplateType,
                previewText = diary.PreviewText,
                // 一般日記的內容
                title = diary.Normal?.Title,
                body = diary.Normal?.Body,
                // 心情日記的內容
                energyValue = diary.MoodDetail?.EnergyValue,
                stressValue = diary.MoodDetail?.StressValue,
                sleepValue = diary.MoodDetail?.SleepValue,
            });
        }

        // 首頁右側 Today 欄位的 Tags
        // GET /api/diary/today-summary?userId=1
        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary(int userId)
        {
            // 1. 取得今天的日期
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 2. 找今天的日記（包含心情資料和標籤）
            var diary = await _db.Diaries
                .Include(d => d.MoodDetail)       // 連接 DiaryMood 表
                .Include(d => d.DiaryTags)        // 連接 DiaryTag 表
                    .ThenInclude(dt => dt.Tag)    // 再從 DiaryTag 連到 Tag 表
                .FirstOrDefaultAsync(d =>
                    d.UserId == userId &&
                    d.DiaryDate == today &&
                    d.Status != "deleted");

            // 3. 今天沒有日記 → 回傳空資料
            if (diary == null)
            {
                return Ok(new
                {
                    hasDiary = false,
                    tags = new List<string>(),
                    moodValue = 0,
                    sleepValue = 0,
                    stressValue = 0,
                });
            }

            // 4. 有日記 → 整理資料回傳
            var tagNames = diary.DiaryTags
                .Select(dt => dt.Tag.TagName)
                .ToList();

            return Ok(new
            {
                hasDiary = true,
                tags = tagNames,
                moodValue = diary.MoodDetail?.EnergyValue ?? 0,
                sleepValue = diary.MoodDetail?.SleepValue ?? 0,
                stressValue = diary.MoodDetail?.StressValue ?? 0,
            });
        }

        // GET /api/diary/today-moods?userId=1
        // 抓今天的心情列表，給右側 Mood 卡片用
        [HttpGet("today-moods")]
        public async Task<IActionResult> GetTodayMoods(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var diary = await _db.Diaries
                .Include(d => d.MoodSelections)
                    .ThenInclude(ms => ms.Mood)
                .FirstOrDefaultAsync(d =>
                    d.UserId == userId &&
                    d.DiaryDate == today &&
                    d.Status != "deleted");

            // 今天沒有日記 → 回傳空陣列
            if (diary == null)
                return Ok(new { moods = new List<object>() });

            // 有日記 → 回傳選擇的心情列表
            var moods = diary.MoodSelections.Select(ms => new
            {
                moodId = ms.MoodId,
                moodName = ms.Mood.MoodName,
                emoji = ms.Mood.MoodEmoji,
            }).ToList();

            return Ok(new { moods });
        }

    }
}
