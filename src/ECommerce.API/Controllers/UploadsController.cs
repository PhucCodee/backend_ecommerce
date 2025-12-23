using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController(IWebHostEnvironment env) : ControllerBase
    {
        private readonly IWebHostEnvironment _env = env;
        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Validate file exists
            if (file == null || file.Length == 0)
                throw new BadRequestException("No file provided");

            // Validate file size
            if (file.Length > MaxFileSize)
                throw new BadRequestException("File size exceeds 5MB limit");

            // Validate file extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(_allowedExtensions, ext) < 0)
                throw new BadRequestException("Invalid file type. Allowed: jpg, jpeg, png, gif, webp");

            // Ensure uploads directory exists
            var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var uniqueName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsPath, uniqueName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Generate URL
            var url = $"/uploads/{uniqueName}";

            // Return success response
            var response = ApiResponse<object>.Ok(new { url }, "File uploaded successfully");
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}