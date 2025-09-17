using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;

namespace TemplateAPIProject.Repositories
{
    public class SQLAuthorRepository : IAuthorRepository
    {
        private readonly AppDBContext _dbContext;
        public SQLAuthorRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AddAuthorRequestDTO AddAuthor(AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorDomainModel = new Author
            {
                FullName = addAuthorRequestDTO.FullName,
            };
            //Use Domain Model to create Author
            _dbContext.Authors.Add(authorDomainModel);
            _dbContext.SaveChanges();
            return addAuthorRequestDTO;
        }

        public Author? DeleteAuthorById(int id)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);
            if (authorDomain != null)
            {
                _dbContext.Authors.Remove(authorDomain);
                _dbContext.SaveChanges();
            }
            return null;
        }

        public List<AuthorDTO> GellAllAuthors()
        {
            //Get Data From Database -Domain Model
            var allAuthorsDomain = _dbContext.Authors.ToList();
            //Map domain models to DTOs
            var allAuthorDTO = new List<AuthorDTO>();
            foreach (var authorDomain in allAuthorsDomain)
            {
                allAuthorDTO.Add(new AuthorDTO()
                {
                    Id = authorDomain.Id,
                    FullName = authorDomain.FullName
                });
            }
            //return DTOs
            return allAuthorDTO;
        }

        public AuthorNoIdDTO GetAuthorById(int id)
        {
            var authorWithIdDomain = _dbContext.Authors.FirstOrDefault(x => x.Id ==
id);
            if (authorWithIdDomain == null)
            {
                return null;
            }
            //Map Domain Model to DTOs
            var authorNoIdDTO = new AuthorNoIdDTO
            {
                FullName = authorWithIdDomain.FullName,
            };
            return authorNoIdDTO;
        }

        public AuthorNoIdDTO UpdateAuthorById(int id, AuthorNoIdDTO authorNoIdDTO)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);
            if (authorDomain != null)
            {
                authorDomain.FullName = authorNoIdDTO.FullName;
                _dbContext.SaveChanges();
            }
            return authorNoIdDTO;

        }
    }

}
