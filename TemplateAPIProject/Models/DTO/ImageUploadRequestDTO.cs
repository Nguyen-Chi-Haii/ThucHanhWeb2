using System.ComponentModel.DataAnnotations;

namespace TemplateAPIProject.Models.DTO
{
    public class ImageUploadRequestDTO
    {
        [Required]
        public IFormFile File { get; set; }
        public string FileName { get; set; }

        public string? FileDescription { get; set; }
    }
}
