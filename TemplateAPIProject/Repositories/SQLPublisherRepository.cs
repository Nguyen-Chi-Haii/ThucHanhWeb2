using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;

namespace TemplateAPIProject.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDBContext _dbContext;
        public SQLPublisherRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public AddPublisherRequestDTO AddPublisher(AddPublisherRequestDTO addPublisherRequestDTO)
        {
            //Kiểm tra tên nhà xuất bản không trùng lặp
            var publisherExists = _dbContext.Publishers.Any(n => n.Name == addPublisherRequestDTO.Name);
            if (publisherExists)
            {
                throw new Exception("Publisher name already exists");
            }
            var publisherDomainModel = new Publisher
            {
                Name = addPublisherRequestDTO.Name,
            };
            //Use Domain Model to create Book
            _dbContext.Publishers.Add(publisherDomainModel);
            _dbContext.SaveChanges();
            return addPublisherRequestDTO;
        }

        public Publisher? DeletePublisherById(int id)
        {
            
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            //Kiểm tra tham chiếu trước khi xóa
            var BookExists = _dbContext.Books.Any(n => n.PublisherID == publisherDomain.Id);
            if (BookExists)
            {
                throw new Exception("Cannot delete this publisher");
            }

            if (publisherDomain != null)
            {
                _dbContext.Publishers.Remove(publisherDomain);
                _dbContext.SaveChanges();
            }
            return null;
        }

        public List<PublisherDTO> GetAllPublishers()
        {
            //Get Data From Database -Domain Model
            var allPublishersDomain = _dbContext.Publishers.ToList();
            //Map domain models to DTOs
            var allPublisherDTO = new List<PublisherDTO>();
            foreach (var publisherDomain in allPublishersDomain)
            {
                allPublisherDTO.Add(new PublisherDTO()
                {
                    Id = publisherDomain.Id,
                    Name = publisherDomain.Name
                });
            }
            return allPublisherDTO;
        }

        public PublisherNoIdDTO GetPublisherById(int id)
        {
            // get book Domain model from Db
            var publisherWithIdDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id ==
           id);
            if (publisherWithIdDomain != null)
            { //Map Domain Model to DTOs
                var publisherNoIdDTO = new PublisherNoIdDTO
                {
                    Name = publisherWithIdDomain.Name,
                };
                return publisherNoIdDTO;
            }
            return null;
        }

        public PublisherNoIdDTO UpdatePublisherById(int id, PublisherNoIdDTO publisherNoIdDTO)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);
            if (publisherDomain != null)
            {
                publisherDomain.Name = publisherNoIdDTO.Name;
                _dbContext.SaveChanges();
            }
            return null;
        }
    }
}
