using Microsoft.EntityFrameworkCore;

namespace TemplateAPIProject.Data
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemplateAPIProject.Models.Domain.Book_Author>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });
            modelBuilder.Entity<TemplateAPIProject.Models.Domain.Book_Author>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.Book_Authors)
                .HasForeignKey(ba => ba.BookId);
            modelBuilder.Entity<TemplateAPIProject.Models.Domain.Book_Author>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.Book_Authors)
                .HasForeignKey(ba => ba.AuthorId);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<TemplateAPIProject.Models.Domain.Book> Books { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Publisher> Publishers { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Author> Authors { get; set; }
        public DbSet<TemplateAPIProject.Models.Domain.Book_Author> Book_Authors { get; set; }
    }
}
