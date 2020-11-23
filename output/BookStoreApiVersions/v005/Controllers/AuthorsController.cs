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
    public class AuthorsController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public AuthorsController(IBookStoreApiRepository repository, IMapper mapper, ILogger<AuthorsController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllAuthors
        [HttpGet]
        [Route("api/Authors")]
        public async Task<ActionResult<Author[]>> GetAllAuthors(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            EntityCollection<Author> dbAuthors= null;
            try
            {
                dbAuthors= await _repository.GetAllAuthorsAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbAuthors == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Author> Authors = new ModelObjectCollection<Data.Models.Author>
            {
                TotalCount = dbAuthors.TotalCount,
                PageNumber = dbAuthors.PageNumber,
                PageSize = dbAuthors.PageSize,
                TotalPages = dbAuthors.TotalPages,
                SortBy = dbAuthors.SortBy,
                NextPageNumber = dbAuthors.NextPageNumber,
                PrevPageNumber = dbAuthors.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Author []>(dbAuthors.Data)
            };
            
            Authors.NextPageUrl = (Authors.PageNumber == Authors.TotalPages) ? "" : ("api/Authors?pageNumber" + Authors.NextPageNumber.ToString())
                +"&pageSize=" + Authors.PageSize.ToString()
                +"&sortBy=" + Authors.SortBy;
            Authors.PrevPageUrl = (Authors.PageNumber == 1) ? "" : ("api/Authors?pageNumber" + Authors.PrevPageNumber.ToString())
                +"&pageSize=" + Authors.PageSize.ToString()
                +"&sortBy=" + Authors.SortBy;
            
            return Ok(Authors);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Authors/{authorId}")]
        public async Task<ActionResult<Author>> GetAuthorByAuthorIdAsync(int authorId)
        {
            Author dbAuthor = await _repository.GetAuthorAsync(authorId);
            
            if (dbAuthor == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Author>(dbAuthor));
        }
        
        // GetByUniqueIndex NonClusteredIndex-20201120-151842
        [HttpGet]
        [Route("api/Authors/{nickName}")]
        public async Task<ActionResult<Author>> GetAuthorByNickNameAsync(string nickName)
        {
            Author dbAuthor = await _repository.GetAuthorByNickNameAsync(nickName);
            if (dbAuthor == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Author>(dbAuthor));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Authors")]
        public async Task<ActionResult<Data.Models.Author>> CreateNewAuthor(Data.Models.AuthorForCreate newAuthor)
        {
            Data.Entities.Author dbNewAuthor = null;
            try
            {
                dbNewAuthor = _mapper.Map<Data.Entities.Author>(newAuthor);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewAuthor == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Author>(dbNewAuthor);
            await _repository.SaveChangesAsync();
            
            Data.Models.Author addedAuthor = _mapper.Map<Data.Models.Author>(dbNewAuthor);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetAuthorByAuthorId", "Authors",  addedAuthor);
            
            return this.Created(url, addedAuthor);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Authors/{authorId}")]
        public async Task<ActionResult<Data.Models.Author>> UpdateAuthor(int authorId, Data.Models.AuthorForUpdate updatedAuthor)
        {
            try
            {
                Data.Entities.Author dbAuthor = await _repository.GetAuthorAsync(authorId);
                
                if (dbAuthor == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedAuthor, dbAuthor);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Author savedAuthor = _mapper.Map<Data.Models.Author>(dbAuthor);
                    return Ok(savedAuthor);            
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
        [Route("api/Authors/{authorId}")]
        public async Task<ActionResult<Data.Models.Author>> PatchAuthor(int authorId, JsonPatchDocument<Data.Models.AuthorForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Author dbAuthor = await _repository.GetAuthorAsync(authorId);
                if (dbAuthor == null)
                {
                    return NotFound();
                }
                
                var updatedAuthor = _mapper.Map<Data.Models.AuthorForUpdate>(dbAuthor);
                patchDocument.ApplyTo(updatedAuthor, ModelState);
                
                _mapper.Map(updatedAuthor, dbAuthor);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Author savedAuthor = _mapper.Map<Data.Models.Author>(await _repository.GetAuthorAsync(authorId));
                    return Ok(savedAuthor);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch author " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Authors/{authorId}")]
        public async Task<IActionResult> DeleteAuthor(int authorId)
        {
            try
            {
                Data.Entities.Author dbAuthor = await _repository.GetAuthorAsync(authorId);
                if (dbAuthor == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Author>(dbAuthor);
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
