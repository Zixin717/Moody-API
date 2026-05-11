using Microsoft.AspNetCore.Mvc;
using Moody_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Moody_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TaskController(AppDbContext db) { _db = db; }

        // GET /api/task/list?userId=1
        // 抓這個用戶所有 Active 的習慣，給首頁左側用
        [HttpGet("list")]
        public async Task<IActionResult> GetUserTasks(int userId)
        {
            var tasks = await _db.HabitTasks
                .Where(t => t.UserId == userId && t.Status == "Active")
                .OrderBy(t => t.CreatedAt)
                .Select(t => new
                {
                    taskId = t.TaskId,
                    title = t.Title,
                    rhythmType = t.RhythmType,
                })
                .ToListAsync();

            return Ok(new { tasks });
        }
    }
}
