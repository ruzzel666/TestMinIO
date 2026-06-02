using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Reactive.Linq;
using TestMinIO.Services;

namespace TestMinIO.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly FileStorageService _storage;

        public FilesController(FileStorageService storage)
        {
            _storage = storage;
        }

        #region Загрузка файлов в хранилище
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            try
            {
                await using var stream = file.OpenReadStream();

                await _storage.UploadAsync(
                    file.FileName,
                    "files",
                    stream,
                    file.ContentType);

                return Ok(new { file = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при загрузке: {ex.Message}");
            }
        }
        #endregion

        #region Скачивание файлов с хранилища
        [HttpGet("download/{name}")]
        public async Task<IActionResult> Download(string name)
        {
            try
            {
                var stream = await _storage.DownloadAsync(name, "files");

                return File(
                    stream,
                    "application/octet-stream",
                    name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при скачивании: {ex.Message}");
            }
        }
        #endregion

        #region Получение списка всех файлов в бакете
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var files = await _storage.GetFilesListAsync("files");
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении списка: {ex.Message}");
            }
        }
        #endregion

        #region Получение временной ссылки на скачивание (Presigned URL)
        [HttpGet("presigned-url/{name}")]
        public async Task<IActionResult> GetPresignedUrl(string name, [FromQuery] int expiryInSeconds = 3600)
        {
            try
            {
                var url = await _storage.GetPresignedUrlAsync("files", name, expiryInSeconds);

                return Ok(new
                {
                    FileName = name,
                    Url = url,
                    ExpiresInSeconds = expiryInSeconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при генерации ссылки: {ex.Message}");
            }
        }
        #endregion
    }
}
