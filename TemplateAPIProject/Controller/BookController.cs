using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;

namespace TemplateAPIProject.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDBContext _dbContext;
        public BookController(AppDBContext context)
        {
            _dbContext = context;
        }


        // GET http://localhost:port/api/get-all-books
        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            // Lấy dữ liệu domain model từ database
            var allBooksDomain = _dbContext.Books;

            // Map domain models sang DTOs
            var allBooksDTO = allBooksDomain
                .Select(book => new BookWithAuthorAndPublisherDTO()
                {
                    Id = book.Id,
                    Description = book.Description,
                    Title = book.Title,
                    IsRead = book.IsRead,
                    DateRead = book.IsRead ? book.DateRead.Value : (DateTime?)null,
                    Rate = book.IsRead ? book.Rate.Value : (int?)null,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors
                                     .Select(n => n.Author.FullName)
                                     .ToList()
                })
                .ToList();

            // Trả về DTOs
            return Ok(allBooksDTO);
        }

        [HttpGet("get-book-by-id/{id}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            // Lấy book từ DB, include Publisher và Authors để tránh null
            var bookDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefault(b => b.Id == id);

            if (bookDomain == null)
            {
                return NotFound();
            }

            // Map Domain Model sang DTO
            var bookDTO = new BookWithAuthorAndPublisherDTO()
            {
                Id = bookDomain.Id,
                Title = bookDomain.Title,
                Description = bookDomain.Description,
                IsRead = bookDomain.IsRead,
                DateRead = bookDomain.IsRead ? bookDomain.DateRead : null,
                Rate = bookDomain.IsRead ? bookDomain.Rate : null,
                Genre = bookDomain.Genre,
                CoverUrl = bookDomain.CoverUrl,
                PublisherName = bookDomain.Publisher?.Name,
                AuthorNames = bookDomain.Book_Authors
                                       .Select(n => n.Author.FullName)
                                       .ToList()
            };

            return Ok(bookDTO);
        }
        [HttpPost("add-book")]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            // Map DTO sang Domain Model
            var bookDomainModel = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.IsRead ? addBookRequestDTO.DateRead : null,
                Rate = addBookRequestDTO.IsRead ? addBookRequestDTO.Rate : null,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = addBookRequestDTO.PublisherID
            };

            // Thêm Book vào DB
            _dbContext.Books.Add(bookDomainModel);
            _dbContext.SaveChanges();

            // Thêm Authors liên kết
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var bookAuthor = new Book_Author()
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = authorId
                };
                _dbContext.Book_Authors.Add(bookAuthor);
            }

            _dbContext.SaveChanges();

            return Ok(new { Message = "Book added successfully", BookId = bookDomainModel.Id });
        }

        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { Message = "Book not found" });
            }

            // Cập nhật thông tin Book
            bookDomain.Title = bookDTO.Title;
            bookDomain.Description = bookDTO.Description;
            bookDomain.IsRead = bookDTO.IsRead;
            bookDomain.DateRead = bookDTO.IsRead ? bookDTO.DateRead : null;
            bookDomain.Rate = bookDTO.IsRead ? bookDTO.Rate : null;
            bookDomain.Genre = bookDTO.Genre;
            bookDomain.CoverUrl = bookDTO.CoverUrl;
            bookDomain.DateAdded = bookDTO.DateAdded;
            bookDomain.PublisherID = bookDTO.PublisherID;

            _dbContext.SaveChanges();

            // Xóa Authors cũ
            var authorDomain = _dbContext.Book_Authors.Where(a => a.BookId == id).ToList();
            if (authorDomain.Any())
            {
                _dbContext.Book_Authors.RemoveRange(authorDomain);
                _dbContext.SaveChanges();
            }

            // Thêm Authors mới
            foreach (var authorId in bookDTO.AuthorIds)
            {
                var bookAuthor = new Book_Author()
                {
                    BookId = id,
                    AuthorId = authorId
                };
                _dbContext.Book_Authors.Add(bookAuthor);
            }

            _dbContext.SaveChanges();

            return Ok(new { Message = "Book updated successfully", Book = bookDTO });
        }

        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(n => n.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { Message = "Book not found" });
            }

            // Xóa quan hệ với Authors trước
            var bookAuthors = _dbContext.Book_Authors.Where(a => a.BookId == id).ToList();
            if (bookAuthors.Any())
            {
                _dbContext.Book_Authors.RemoveRange(bookAuthors);
            }

            // Xóa chính Book
            _dbContext.Books.Remove(bookDomain);
            _dbContext.SaveChanges();

            return Ok(new { Message = "Book deleted successfully" });
        }

    }

}
