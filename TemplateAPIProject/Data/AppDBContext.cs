using Microsoft.EntityFrameworkCore;
using TemplateAPIProject.Models.Domain;

namespace TemplateAPIProject.Data
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book_Author>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<Book_Author>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.Book_Authors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<Book_Author>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.Book_Authors)
                .HasForeignKey(ba => ba.AuthorId);
        }

        public DbSet<TemplateAPIProject.Models.Domain.Book> Books { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Publisher> Publishers { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Author> Authors { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Book_Author> Book_Authors { get; set; }

        public DbSet<TemplateAPIProject.Models.Domain.Image> Images { get; set; }
    }
}
