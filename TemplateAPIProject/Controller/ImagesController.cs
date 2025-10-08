using Microsoft.AspNetCore.Mvc;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Repositories;

namespace TemplateAPIProject.Controller
{
    public class ImagesController : ControllerBase
    {
       private readonly IImageRepository _imageRepository;
       private readonly ILogger<ImagesController> _logger;
         public ImagesController(IImageRepository imageRepository, ILogger<ImagesController> logger)
         {
              _imageRepository = imageRepository;
              _logger = logger;
        }

        [HttpPost]
        [Route("api/images/upload")]
        public IActionResult Upload([FromForm] ImageUploadRequestDTO image)
        {
            ValidateFileUpload(image);
            if (ModelState.IsValid)
            {
                var imageDomainModel = new Image
                {
                   File = image.File,
                   FileName = image.FileName,
                   FileDescription = image.FileDescription,
                   FileExtension = Path.GetExtension(image.File.FileName),
                   FileSizeInBytes = image.File.Length
                };
                _imageRepository.Upload(imageDomainModel);
                return Ok(imageDomainModel);
            }
            return BadRequest(ModelState);
        }


        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            var images = _imageRepository.GetAll();
            return Ok(images);
        }

        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            try
            {
                var (bytes, type, name) = _imageRepository.Download(id);
                return File(bytes, type, name);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private void ValidateFileUpload(ImageUploadRequestDTO image)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExtensions.Contains(Path.GetExtension(image.File.FileName)))
            {
                ModelState.AddModelError("File", "Unsupported file type. Allowed types are: .jpg, .jpeg, .png, .gif");
            }

            if (image.File.Length > 5 * 1024 * 1024) // 5MB limit
            {
                ModelState.AddModelError("File", "File size exceeds the 5MB limit.");
            }
        }

    }
}
