using Microsoft.AspNetCore.Mvc;
using Moody_backend.Data;
using Moody_backend.Models;

namespace Moody_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MediaController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia([FromForm] FileUploadRequest request)
        {
            if (request.file == null || request.file.Length == 0)
                return BadRequest("請選擇要上傳的圖片");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string mediaId = Guid.NewGuid().ToString("N").Substring(0, 20);
            string fileExtension = Path.GetExtension(request.file.FileName);
            string newFileName = $"{mediaId}{fileExtension}";
            string filePath = Path.Combine(uploadsFolder, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.file.CopyToAsync(stream);
            }

            var media = new DiaryMedia
            {
                MediaId = mediaId,
                DiaryId = request.diaryId,
                MediaType = "image",
                FileUrl = $"/uploads/{newFileName}",
                CreatedAt = DateTime.UtcNow
            };

            _db.DiaryMedias.Add(media);
            await _db.SaveChangesAsync();

            return Ok(new { message = "上傳成功！", fileUrl = media.FileUrl });
        }


    }

    public class FileUploadRequest
    {
        public IFormFile file { get; set; }
        public long diaryId { get; set; }
    }
}
