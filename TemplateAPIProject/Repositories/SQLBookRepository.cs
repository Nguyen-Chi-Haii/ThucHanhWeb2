using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Repositories.TemplateAPIProject.Repositories;

namespace TemplateAPIProject.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDBContext _dbContext;

        public SQLBookRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy toàn bộ sách (async)
        public async Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync(
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 1000)
        {
            // Start with the base query.
            var allBooksQuery = _dbContext.Books.AsQueryable();

            // Apply filtering.
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooksQuery = allBooksQuery.Where(book => book.Title.Contains(filterQuery));
                }
                // You can add more filtering options here.
            }

            // Apply sorting.
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooksQuery = isAscending ? allBooksQuery.OrderBy(x => x.Title) :
                        allBooksQuery.OrderByDescending(x => x.Title);
                }
            }

            // Apply pagination.
            var skipResults = (pageNumber - 1) * pageSize;
            var pagedAndSortedBooksQuery = allBooksQuery.Skip(skipResults).Take(pageSize);

            // Project the results to the DTO and execute the query.
            return await pagedAndSortedBooksQuery
                .Select(book => new BookWithAuthorAndPublisherDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.IsRead ? book.DateRead.Value : null,
                    Rate = book.IsRead ? book.Rate.Value : null,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                })
                .ToListAsync();
        }

        // Lấy sách theo ID (async)
        public async Task<BookWithAuthorAndPublisherDTO?> GetBookByIdAsync(int id)
        {
            return await _dbContext.Books
                .Where(b => b.Id == id)
                .Select(book => new BookWithAuthorAndPublisherDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.DateRead,
                    Rate = book.Rate,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // Thêm sách (async)
        public async Task<AddBookRequestDTO> AddBookAsync(AddBookRequestDTO addBookRequestDTO)
        {
            // Bắt đầu một transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra Publisher tồn tại
                var publisherExists = await _dbContext.Publishers
                    .AnyAsync(p => p.Id == addBookRequestDTO.PublisherID);

                if (!publisherExists)
                {
                    throw new Exception($"Publisher with ID {addBookRequestDTO.PublisherID} does not exist");
                }

                // 2. Kiểm tra Authors tồn tại
                var existingAuthors = await _dbContext.Authors
                    .Where(a => addBookRequestDTO.AuthorIds.Contains(a.Id))
                    .ToListAsync();

                if (existingAuthors.Count != addBookRequestDTO.AuthorIds.Count)
                {
                    var missingAuthors = addBookRequestDTO.AuthorIds
                        .Except(existingAuthors.Select(a => a.Id))
                        .ToList();

                    throw new Exception($"These Author IDs do not exist: {string.Join(", ", missingAuthors)}");
                }
                if (addBookRequestDTO.AuthorIds == null || !addBookRequestDTO.AuthorIds.Any())
                {
                    throw new Exception("A book must have at least one author.");
                }
                //3. Kiểm tra tiêu đề sách không chứa kí tự đặc biệt
                // Regex: chỉ cho phép chữ cái, số, khoảng trắng, dấu gạch ngang (-) và gạch dưới (_)
                var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\s\-_]+$");

                if (!regex.IsMatch(addBookRequestDTO.Title))
                {
                    throw new Exception("Book title must not contain special characters");
                }
                //Kiểm tra số lượng sách đã xuất bản trong năm bởi 1 nhà xuất bản
                var NumberBooksPublicbyPublisher = _dbContext.Books
                    .Count(b => b.PublisherID == addBookRequestDTO.PublisherID && b.DateAdded.Year == DateTime.Now.Year);
                if (NumberBooksPublicbyPublisher >= 5)
                {
                    throw new Exception($"Publisher with ID {addBookRequestDTO.PublisherID} cannot publish more than 5 books in a year.");
                }
                //Kiem tra trung ten sach va nha xuat ban
                var BookTitleExists = _dbContext.Books.Any(t => t.Title == addBookRequestDTO.Title);
                if(publisherExists && BookTitleExists)
                {
                    throw new Exception("This publisher cannot public 2 book with same name");
                }

                var bookDomainModel = new Book
                {
                    Title = addBookRequestDTO.Title,
                    Description = addBookRequestDTO.Description,
                    IsRead = addBookRequestDTO.IsRead,
                    DateRead = addBookRequestDTO.DateRead,
                    Rate = addBookRequestDTO.Rate,
                    Genre = addBookRequestDTO.Genre,
                    CoverUrl = addBookRequestDTO.CoverUrl,
                    DateAdded = addBookRequestDTO.DateAdded,
                    PublisherID = addBookRequestDTO.PublisherID
                };
                await _dbContext.Books.AddAsync(bookDomainModel);
                await _dbContext.SaveChangesAsync();

                // 5. Gán tác giả
                foreach (var authorId in addBookRequestDTO.AuthorIds)
                {
                    var bookAuthorExists = await _dbContext.Book_Authors
                        .AnyAsync(ba => ba.Book.Title == bookDomainModel.Title && ba.AuthorId == authorId);

                    if (bookAuthorExists)
                    {
                        // Nếu có lỗi, ném ngoại lệ và transaction sẽ tự động rollback
                        return null; // hoặc false để báo lỗi
                    }

                    var bookAuthor = new Book_Author
                    {
                        BookId = bookDomainModel.Id,
                        AuthorId = authorId
                    };

                    var NumbersOfBooksByAuthor = await _dbContext.Book_Authors
                        .CountAsync(ba => ba.AuthorId == authorId);
                    if (NumbersOfBooksByAuthor >= 3)
                    {
                        throw new Exception($"Author with ID {authorId} cannot be assigned to more than 3 books.");
                    }
                    _dbContext.Book_Authors.Add(bookAuthor);
                }

                // 6. Lưu các bản ghi Book_Author
                await _dbContext.SaveChangesAsync();

                // 7. Hoàn thành transaction
                await transaction.CommitAsync();

                return addBookRequestDTO;
            }
            catch (Exception)
            {
                // Nếu có bất kỳ lỗi nào, transaction sẽ tự động được rollback
                await transaction.RollbackAsync();
                throw; // Ném lại ngoại lệ để lỗi được xử lý ở lớp cao hơn
            }
        }

        // Update sách (async)
        public async Task<AddBookRequestDTO?> UpdateBookByIdAsync(int id, AddBookRequestDTO bookDTO)
        {
            var bookDomain = await _dbContext.Books.FirstOrDefaultAsync(n => n.Id == id);
            if (bookDomain == null) return null;

            bookDomain.Title = bookDTO.Title;
            bookDomain.Description = bookDTO.Description;
            bookDomain.IsRead = bookDTO.IsRead;
            bookDomain.DateRead = bookDTO.DateRead;
            bookDomain.Rate = bookDTO.Rate;
            bookDomain.Genre = bookDTO.Genre;
            bookDomain.CoverUrl = bookDTO.CoverUrl;
            bookDomain.DateAdded = bookDTO.DateAdded;
            bookDomain.PublisherID = bookDTO.PublisherID;

            await _dbContext.SaveChangesAsync();

            var authorDomain = await _dbContext.Book_Authors.Where(a => a.BookId == id).ToListAsync();
            _dbContext.Book_Authors.RemoveRange(authorDomain);
            await _dbContext.SaveChangesAsync();

            foreach (var authorId in bookDTO.AuthorIds)
            {
                var bookAuthor = new Book_Author
                {
                    BookId = id,
                    AuthorId = authorId
                };
                await _dbContext.Book_Authors.AddAsync(bookAuthor);
            }
            await _dbContext.SaveChangesAsync();

            return bookDTO;
        }

        // Xóa sách (async)
        public async Task<Book?> DeleteBookByIdAsync(int id)
        {
            var bookDomain = await _dbContext.Books.FirstOrDefaultAsync(n => n.Id == id);
            if (bookDomain == null) return null;

            _dbContext.Books.Remove(bookDomain);
            await _dbContext.SaveChangesAsync();

            return bookDomain;
        }

    }
}
