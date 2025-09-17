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
    public async Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync()
        {
            return await _dbContext.Books
                .Select(book => new BookWithAuthorAndPublisherDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.IsRead ? book.DateRead : null,
                    Rate = book.IsRead ? book.Rate : null,
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

            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var bookAuthor = new Book_Author
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = authorId
                };
                await _dbContext.Book_Authors.AddAsync(bookAuthor);
            }
            await _dbContext.SaveChangesAsync();

            return addBookRequestDTO;
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
