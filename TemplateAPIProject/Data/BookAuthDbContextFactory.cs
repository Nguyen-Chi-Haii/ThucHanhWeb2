using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TemplateAPIProject.Data
{
    public class BookAuthDbContextFactory : IDesignTimeDbContextFactory<BookAuthDbContext>
    {
        public BookAuthDbContext CreateDbContext(string[] args)
        {
            // 👉 Dùng thẳng connection string để tránh lỗi đọc file appsettings.json
            var connectionString =
                "Server=PC;Database=BookAuthorDB;User Id=sa;Password=123456789;Trusted_Connection=False;Encrypt=False;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<BookAuthDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookAuthDbContext(optionsBuilder.Options);
        }
    }
}
