using global::TemplateAPIProject.Models.Domain;
using global::TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;
namespace TemplateAPIProject.Repositories
{
   

    namespace TemplateAPIProject.Repositories
    {
        public interface IBookRepository
        {
            // Lấy tất cả sách
            Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync(
            string? filterOn = null, string? filterQuery = null,
            string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 1000);

            // Lấy sách theo ID
            Task<BookWithAuthorAndPublisherDTO?> GetBookByIdAsync(int id);

            // Thêm sách mới
            Task<AddBookRequestDTO> AddBookAsync(AddBookRequestDTO addBookRequestDTO);

            // Cập nhật sách theo ID
            Task<AddBookRequestDTO?> UpdateBookByIdAsync(int id, AddBookRequestDTO bookDTO);

            // Xóa sách theo ID
            Task<Book?> DeleteBookByIdAsync(int id);
        }
    }

}
