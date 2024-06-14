using Microsoft.AspNetCore.Mvc;

namespace FoodHealthChecker.Controller
{
    [ApiController]
    [Route("image")]
    public class ImageUploadController : ControllerBase
    {

        [HttpPost("upload")]
        public async Task UploadFile([FromForm] IFormFile file)
        {
            string filePath = string.Empty;
            try
            {
                // Ensure the TempImages folder exists and is empty
                var folderPath = Path.Combine("wwwroot", "temp");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                filePath = Path.Combine(folderPath, file.FileName);
                await using FileStream stream = new(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
