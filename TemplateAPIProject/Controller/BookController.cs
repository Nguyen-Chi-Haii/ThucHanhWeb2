using Catel.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Repositories;
using TemplateAPIProject.Repositories.TemplateAPIProject.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TemplateAPIProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookRepository bookRepository, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        // GET http://localhost:port/api/book/get-all-books
        [HttpGet("get-all-books")]
        [HttpGet]
        [Authorize(Roles = "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool isAscending,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            _logger.LogInformation("Getting all books");
            _logger.LogWarning("This is a warning message");
            _logger.LogError("This is an error message");
            var allBooks = await _bookRepository.GetAllBooksAsync(filterOn, filterQuery, sortBy,
isAscending);
            _logger.LogInformation($"Finished GetAllBook request with data { JsonSerializer.Serialize(allBooks)} ");
            return Ok(allBooks);
        }

        // GET http://localhost:port/api/book/get-book-by-id/1
        [HttpGet("get-book-by-id/{id}")]
        [Authorize(Roles = "Read")]
        public async Task<IActionResult> GetBookById([FromRoute] int id)
        {
            var bookDTO = await _bookRepository.GetBookByIdAsync(id);

            if (bookDTO == null)
            {
                return NotFound(new { Message = "Book not found" });
            }

            return Ok(bookDTO);
        }

        // POST http://localhost:port/api/book/add-book
        [HttpPost("add-book")]
        [Authorize(Roles ="Read,Write")]

        public async Task<IActionResult> AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdBook = await _bookRepository.AddBookAsync(addBookRequestDTO);
                if (createdBook == null)
                {
                    return Conflict(new { message = "Author is already assigned to this book." });
                }
                return Ok(new { Message = "Book added successfully", Book = createdBook });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT http://localhost:port/api/book/update-book-by-id/1
        [HttpPut("update-book-by-id/{id}")]
        [Authorize(Roles ="Read,Write")]
        public async Task<IActionResult> UpdateBookById([FromRoute] int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var updatedBook = await _bookRepository.UpdateBookByIdAsync(id, bookDTO);

            if (updatedBook == null)
            {
                return NotFound(new { Message = "Book not found" });
            }

            return Ok(new { Message = "Book updated successfully", Book = updatedBook });
        }

        // DELETE http://localhost:port/api/book/delete-book-by-id/1
        [HttpDelete("delete-book-by-id/{id}")]
        [Authorize(Roles ="Read,Write")]
        public async Task<IActionResult> DeleteBookById([FromRoute] int id)
        {
            var deletedBook = await _bookRepository.DeleteBookByIdAsync(id);

            if (deletedBook == null)
            {
                return NotFound(new { Message = "Book not found" });
            }

            return Ok(new { Message = "Book deleted successfully" });
        }
    }
}
