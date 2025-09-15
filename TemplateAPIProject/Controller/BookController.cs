using Microsoft.AspNetCore.Mvc;
using TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Repositories;
using TemplateAPIProject.Repositories.TemplateAPIProject.Repositories;

namespace TemplateAPIProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BookController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        // GET http://localhost:port/api/book/get-all-books
        [HttpGet("get-all-books")]
        public async Task<IActionResult> GetAll()
        {
            var allBooksDTO = await _bookRepository.GetAllBooksAsync();
            return Ok(allBooksDTO);
        }

        // GET http://localhost:port/api/book/get-book-by-id/1
        [HttpGet("get-book-by-id/{id}")]
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
        public async Task<IActionResult> AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            var createdBook = await _bookRepository.AddBookAsync(addBookRequestDTO);
            return Ok(new { Message = "Book added successfully", Book = createdBook });
        }

        // PUT http://localhost:port/api/book/update-book-by-id/1
        [HttpPut("update-book-by-id/{id}")]
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
