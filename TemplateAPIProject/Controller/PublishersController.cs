using Microsoft.AspNetCore.Mvc;
using TemplateAPIProject.Data;
using TemplateAPIProject.Models.Domain;
using TemplateAPIProject.Models.DTO;
using TemplateAPIProject.Repositories;

namespace TemplateAPIProject.Controller
{
    public class PublishersController : ControllerBase
    {
        private readonly AppDBContext _dbContext;
        private readonly IPublisherRepository _publisherRepository;
        public PublishersController(AppDBContext dbContext, IPublisherRepository
       publisherRepository)
        {
            _dbContext = dbContext;
            _publisherRepository = publisherRepository;
        }
        [HttpGet("get-all-publisher")]
        public IActionResult GetAllPublisher()
        {
            var allPublishers = _publisherRepository.GetAllPublishers();
            return Ok(allPublishers);
        }
        [HttpGet("get-publisher-by-id")]
        public IActionResult GetPublisherById(int id)
        {
            var publisherWithId = _publisherRepository.GetPublisherById(id);
            return Ok(publisherWithId);
        }
        [HttpPost("add-publisher")]
        public IActionResult AddPublisher([FromBody] AddPublisherRequestDTO
       addPublisherRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var publisherAdd = _publisherRepository.AddPublisher(addPublisherRequestDTO);
                return Ok(publisherAdd);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            
        }
        [HttpPut("update-publisher-by-id/{id}")]
        public IActionResult UpdatePublisherById(int id, [FromBody] PublisherNoIdDTO
       publisherDTO)
        {
            var publisherUpdate = _publisherRepository.UpdatePublisherById(id,
           publisherDTO);

            return Ok(publisherUpdate);
        }
        [HttpDelete("delete-publisher-by-id/{id}")]
        public IActionResult DeletePublisherById(int id)
        {
            try
            {
                var publisherDelete = _publisherRepository.DeletePublisherById(id);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

}
