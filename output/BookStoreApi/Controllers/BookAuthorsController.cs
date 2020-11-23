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
    public class BookAuthorsController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookAuthorsController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public BookAuthorsController(IBookStoreApiRepository repository, IMapper mapper, ILogger<BookAuthorsController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllBookAuthors
        [HttpGet]
        [Route("api/BookAuthors")]
        public async Task<ActionResult<BookAuthor[]>> GetAllBookAuthors(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            EntityCollection<BookAuthor> dbBookAuthors= null;
            try
            {
                dbBookAuthors= await _repository.GetAllBookAuthorsAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBookAuthors == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.BookAuthor> BookAuthors = new ModelObjectCollection<Data.Models.BookAuthor>
            {
                TotalCount = dbBookAuthors.TotalCount,
                PageNumber = dbBookAuthors.PageNumber,
                PageSize = dbBookAuthors.PageSize,
                TotalPages = dbBookAuthors.TotalPages,
                SortBy = dbBookAuthors.SortBy,
                NextPageNumber = dbBookAuthors.NextPageNumber,
                PrevPageNumber = dbBookAuthors.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.BookAuthor []>(dbBookAuthors.Data)
            };
            
            BookAuthors.NextPageUrl = (BookAuthors.PageNumber == BookAuthors.TotalPages) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.NextPageNumber.ToString())
                +"&pageSize=" + BookAuthors.PageSize.ToString()
                +"&sortBy=" + BookAuthors.SortBy;
            BookAuthors.PrevPageUrl = (BookAuthors.PageNumber == 1) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.PrevPageNumber.ToString())
                +"&pageSize=" + BookAuthors.PageSize.ToString()
                +"&sortBy=" + BookAuthors.SortBy;
            
            return Ok(BookAuthors);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/BookAuthors/{authorId}")]
        public async Task<ActionResult<BookAuthor>> GetBookAuthorByAuthorIdAsync(int authorId)
        {
            BookAuthor dbBookAuthor = await _repository.GetBookAuthorAsync(authorId);
            
            if (dbBookAuthor == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.BookAuthor>(dbBookAuthor));
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Authors/{authorId}/BookAuthors")]
        public async Task<ActionResult<BookAuthor[]>> GetBookAuthorsByAuthorIdAsync(int authorId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            EntityCollection<BookAuthor> dbBookAuthors = null;
            try
            {
                dbBookAuthors = await _repository.GetBookAuthorsByAuthorIdAsync(authorId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBookAuthors == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.BookAuthor> BookAuthors = new ModelObjectCollection<Data.Models.BookAuthor>
            {
                TotalCount = dbBookAuthors.TotalCount,
                PageNumber = dbBookAuthors.PageNumber,
                PageSize = dbBookAuthors.PageSize,
                TotalPages = dbBookAuthors.TotalPages,
                SortBy = dbBookAuthors.SortBy,
                NextPageNumber = dbBookAuthors.NextPageNumber,
                PrevPageNumber = dbBookAuthors.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.BookAuthor []>(dbBookAuthors.Data)
            };
            
            BookAuthors.NextPageUrl = (BookAuthors.PageNumber == BookAuthors.TotalPages) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.NextPageNumber.ToString())
                + "&pageSize=" + BookAuthors.PageSize.ToString()
                + "&sortBy=" + BookAuthors.SortBy;
            BookAuthors.PrevPageUrl = (BookAuthors.PageNumber == 1) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.PrevPageNumber.ToString())
                + "&pageSize=" + BookAuthors.PageSize.ToString()
                + "&sortBy=" + BookAuthors.SortBy;
            
            return Ok(BookAuthors);
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Books/{bookId}/BookAuthors")]
        public async Task<ActionResult<BookAuthor[]>> GetBookAuthorsByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "AuthorId Desc")
        {
            EntityCollection<BookAuthor> dbBookAuthors = null;
            try
            {
                dbBookAuthors = await _repository.GetBookAuthorsByBookIdAsync(bookId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBookAuthors == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.BookAuthor> BookAuthors = new ModelObjectCollection<Data.Models.BookAuthor>
            {
                TotalCount = dbBookAuthors.TotalCount,
                PageNumber = dbBookAuthors.PageNumber,
                PageSize = dbBookAuthors.PageSize,
                TotalPages = dbBookAuthors.TotalPages,
                SortBy = dbBookAuthors.SortBy,
                NextPageNumber = dbBookAuthors.NextPageNumber,
                PrevPageNumber = dbBookAuthors.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.BookAuthor []>(dbBookAuthors.Data)
            };
            
            BookAuthors.NextPageUrl = (BookAuthors.PageNumber == BookAuthors.TotalPages) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.NextPageNumber.ToString())
                + "&pageSize=" + BookAuthors.PageSize.ToString()
                + "&sortBy=" + BookAuthors.SortBy;
            BookAuthors.PrevPageUrl = (BookAuthors.PageNumber == 1) ? "" : ("api/BookAuthors?pageNumber" + BookAuthors.PrevPageNumber.ToString())
                + "&pageSize=" + BookAuthors.PageSize.ToString()
                + "&sortBy=" + BookAuthors.SortBy;
            
            return Ok(BookAuthors);
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/BookAuthors")]
        public async Task<ActionResult<Data.Models.BookAuthor>> CreateNewBookAuthor(Data.Models.BookAuthorForCreate newBookAuthor)
        {
            Data.Entities.BookAuthor dbNewBookAuthor = null;
            try
            {
                dbNewBookAuthor = _mapper.Map<Data.Entities.BookAuthor>(newBookAuthor);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewBookAuthor == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.BookAuthor>(dbNewBookAuthor);
            await _repository.SaveChangesAsync();
            
            Data.Models.BookAuthor addedBookAuthor = _mapper.Map<Data.Models.BookAuthor>(dbNewBookAuthor);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetBookAuthorByAuthorId", "BookAuthors",  addedBookAuthor);
            
            return this.Created(url, addedBookAuthor);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/BookAuthors/{authorId}")]
        public async Task<ActionResult<Data.Models.BookAuthor>> UpdateBookAuthor(int authorId, Data.Models.BookAuthorForUpdate updatedBookAuthor)
        {
            try
            {
                Data.Entities.BookAuthor dbBookAuthor = await _repository.GetBookAuthorAsync(authorId);
                
                if (dbBookAuthor == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedBookAuthor, dbBookAuthor);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.BookAuthor savedBookAuthor = _mapper.Map<Data.Models.BookAuthor>(dbBookAuthor);
                    return Ok(savedBookAuthor);            
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
        [Route("api/BookAuthors/{authorId}")]
        public async Task<ActionResult<Data.Models.BookAuthor>> PatchBookAuthor(int authorId, JsonPatchDocument<Data.Models.BookAuthorForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.BookAuthor dbBookAuthor = await _repository.GetBookAuthorAsync(authorId);
                if (dbBookAuthor == null)
                {
                    return NotFound();
                }
                
                var updatedBookAuthor = _mapper.Map<Data.Models.BookAuthorForUpdate>(dbBookAuthor);
                patchDocument.ApplyTo(updatedBookAuthor, ModelState);
                
                _mapper.Map(updatedBookAuthor, dbBookAuthor);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.BookAuthor savedBookAuthor = _mapper.Map<Data.Models.BookAuthor>(await _repository.GetBookAuthorAsync(authorId));
                    return Ok(savedBookAuthor);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch bookAuthor " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/BookAuthors/{authorId}")]
        public async Task<IActionResult> DeleteBookAuthor(int authorId)
        {
            try
            {
                Data.Entities.BookAuthor dbBookAuthor = await _repository.GetBookAuthorAsync(authorId);
                if (dbBookAuthor == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.BookAuthor>(dbBookAuthor);
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
