using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class UploadController : ControllerBase
{
    private readonly IImageService _imageService;

    public UploadController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "Others")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "Vui lòng chọn một file hợp lệ." });
        }

        // Validate file type
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (Array.IndexOf(allowedExtensions, extension) == -1)
        {
            return BadRequest(new { Message = "Định dạng ảnh không hỗ trợ. Chỉ chấp nhận jpg, png, gif, webp." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _imageService.UploadImageAsync(stream, file.FileName, folder);
            return Ok(new { Url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Lỗi khi upload ảnh: " + ex.Message });
        }
    }
}
