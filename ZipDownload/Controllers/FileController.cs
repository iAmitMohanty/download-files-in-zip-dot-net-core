#nullable disable

using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace ZipDownload.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public FilesController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        [Route("download-zip")]
        public IActionResult DownloadFiles()
        {
            try
            {
                var folderPath = Path.Combine(_hostEnvironment.ContentRootPath, "FilesToDownload");

                // Ensure the folder exists
                if (!Directory.Exists(folderPath))
                    return NotFound("Folder not found.");

                // Get a list of files in the folder
                var files = Directory.GetFiles(folderPath);

                if (files.Length == 0)
                    return NotFound("No files found to download.");

                // Create a temporary memory stream to hold the zip archive
                using (var memoryStream = new MemoryStream())
                {
                    // Create a new zip archive
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);

                            // Create a new entry in the zip archive for each file
                            var entry = zipArchive.CreateEntry(fileInfo.Name);

                            // Write the file contents into the entry
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Return the zip file as a byte array
                    return File(memoryStream.ToArray(), "application/zip", "files.zip");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
