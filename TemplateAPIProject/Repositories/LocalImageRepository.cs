using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;

namespace TemplateAPIProject.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDBContext _dbContext;
        private readonly string _imageFolderPath;
        public LocalImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, AppDBContext appDBContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = appDBContext;
        }

        public (byte[], string, string) Download(int id)
        {
            var fileRecord = _dbContext.Images.FirstOrDefault(x => x.Id == id);
            if (fileRecord == null)
                throw new FileNotFoundException($"No file record found for id {id}");

            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            var filePath = Path.Combine(folderPath, $"{fileRecord.FileName}{fileRecord.FileExtension}");

            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"File not found on server: {filePath}");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";
            var fileName = $"{fileRecord.FileName}{fileRecord.FileExtension}";

            return (fileBytes, contentType, fileName);
        }


        public Image Upload(Image image)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

            // Tự tạo folder nếu chưa có
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var localFilePath = Path.Combine(folderPath, $"{image.FileName}{image.FileExtension}");

            // Upload image vào local folder
            using (var stream = new FileStream(localFilePath, FileMode.Create))
            {
                image.File.CopyTo(stream);
            }

            // Tạo URL truy cập ảnh
            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://" +
                              $"{_httpContextAccessor.HttpContext.Request.Host}/Images/" +
                              $"{image.FileName}{image.FileExtension}";

            image.FilePath = urlFilePath;

            // Lưu vào DB
            _dbContext.Images.Add(image);
            _dbContext.SaveChanges();

            return image;
        }

        List<Image> IImageRepository.GetAll()
        {
           var allImages = _dbContext.Images.ToList();
           return allImages;
        }
    }
}
