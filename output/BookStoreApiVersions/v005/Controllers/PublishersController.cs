using System.Linq;
using System.Linq.Dynamic.Core.Exceptions;
using System.Threading.Tasks;
using AutoMapper;
using BookStoreApi.Data;
using Microsoft.AspNetCore.Http;
using BookStoreApi.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.JsonPatch;

namespace BookStoreApi.Controllers
{
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<PublishersController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public PublishersController(IBookStoreApiRepository repository, IMapper mapper, ILogger<PublishersController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllPublishers
        [HttpGet]
        [Route("api/Publishers")]
        public async Task<ActionResult<Publisher[]>> GetAllPublishers(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "PubId Desc")
        {
            EntityCollection<Publisher> dbPublishers= null;
            try
            {
                dbPublishers= await _repository.GetAllPublishersAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbPublishers == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Publisher> Publishers = new ModelObjectCollection<Data.Models.Publisher>
            {
                TotalCount = dbPublishers.TotalCount,
                PageNumber = dbPublishers.PageNumber,
                PageSize = dbPublishers.PageSize,
                TotalPages = dbPublishers.TotalPages,
                SortBy = dbPublishers.SortBy,
                NextPageNumber = dbPublishers.NextPageNumber,
                PrevPageNumber = dbPublishers.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Publisher []>(dbPublishers.Data)
            };
            
            Publishers.NextPageUrl = (Publishers.PageNumber == Publishers.TotalPages) ? "" : ("api/Publishers?pageNumber" + Publishers.NextPageNumber.ToString())
                +"&pageSize=" + Publishers.PageSize.ToString()
                +"&sortBy=" + Publishers.SortBy;
            Publishers.PrevPageUrl = (Publishers.PageNumber == 1) ? "" : ("api/Publishers?pageNumber" + Publishers.PrevPageNumber.ToString())
                +"&pageSize=" + Publishers.PageSize.ToString()
                +"&sortBy=" + Publishers.SortBy;
            
            return Ok(Publishers);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Publishers/{pubId}")]
        public async Task<ActionResult<Publisher>> GetPublisherByPubIdAsync(int pubId)
        {
            Publisher dbPublisher = await _repository.GetPublisherAsync(pubId);
            
            if (dbPublisher == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Publisher>(dbPublisher));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Publishers")]
        public async Task<ActionResult<Data.Models.Publisher>> CreateNewPublisher(Data.Models.PublisherForCreate newPublisher)
        {
            Data.Entities.Publisher dbNewPublisher = null;
            try
            {
                dbNewPublisher = _mapper.Map<Data.Entities.Publisher>(newPublisher);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewPublisher == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Publisher>(dbNewPublisher);
            await _repository.SaveChangesAsync();
            
            Data.Models.Publisher addedPublisher = _mapper.Map<Data.Models.Publisher>(dbNewPublisher);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetPublisherByPubId", "Publishers",  addedPublisher);
            
            return this.Created(url, addedPublisher);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Publishers/{pubId}")]
        public async Task<ActionResult<Data.Models.Publisher>> UpdatePublisher(int pubId, Data.Models.PublisherForUpdate updatedPublisher)
        {
            try
            {
                Data.Entities.Publisher dbPublisher = await _repository.GetPublisherAsync(pubId);
                
                if (dbPublisher == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedPublisher, dbPublisher);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Publisher savedPublisher = _mapper.Map<Data.Models.Publisher>(dbPublisher);
                    return Ok(savedPublisher);            
                }
                else
                {
                    return BadRequest("Failed to update.");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database exception: " + ex.Message);
            }
        }
        
        // HttpPatch partial update
        [HttpPatch]
        [Route("api/Publishers/{pubId}")]
        public async Task<ActionResult<Data.Models.Publisher>> PatchPublisher(int pubId, JsonPatchDocument<Data.Models.PublisherForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Publisher dbPublisher = await _repository.GetPublisherAsync(pubId);
                if (dbPublisher == null)
                {
                    return NotFound();
                }
                
                var updatedPublisher = _mapper.Map<Data.Models.PublisherForUpdate>(dbPublisher);
                patchDocument.ApplyTo(updatedPublisher, ModelState);
                
                _mapper.Map(updatedPublisher, dbPublisher);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Publisher savedPublisher = _mapper.Map<Data.Models.Publisher>(await _repository.GetPublisherAsync(pubId));
                    return Ok(savedPublisher);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch publisher " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Publishers/{pubId}")]
        public async Task<IActionResult> DeletePublisher(int pubId)
        {
            try
            {
                Data.Entities.Publisher dbPublisher = await _repository.GetPublisherAsync(pubId);
                if (dbPublisher == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Publisher>(dbPublisher);
                await _repository.SaveChangesAsync();
                
                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database exception: " + ex.Message);
            }
        }
    }
}
