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
    public class BooksController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BooksController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public BooksController(IBookStoreApiRepository repository, IMapper mapper, ILogger<BooksController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllBooks
        [HttpGet]
        [Route("api/Books")]
        public async Task<ActionResult<Book[]>> GetAllBooks(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc")
        {
            EntityCollection<Book> dbBooks= null;
            try
            {
                dbBooks= await _repository.GetAllBooksAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBooks == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Book> Books = new ModelObjectCollection<Data.Models.Book>
            {
                TotalCount = dbBooks.TotalCount,
                PageNumber = dbBooks.PageNumber,
                PageSize = dbBooks.PageSize,
                TotalPages = dbBooks.TotalPages,
                SortBy = dbBooks.SortBy,
                NextPageNumber = dbBooks.NextPageNumber,
                PrevPageNumber = dbBooks.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Book []>(dbBooks.Data)
            };
            
            Books.NextPageUrl = (Books.PageNumber == Books.TotalPages) ? "" : ("api/Books?pageNumber" + Books.NextPageNumber.ToString())
                +"&pageSize=" + Books.PageSize.ToString()
                +"&sortBy=" + Books.SortBy;
            Books.PrevPageUrl = (Books.PageNumber == 1) ? "" : ("api/Books?pageNumber" + Books.PrevPageNumber.ToString())
                +"&pageSize=" + Books.PageSize.ToString()
                +"&sortBy=" + Books.SortBy;
            
            return Ok(Books);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Books/{bookId}")]
        public async Task<ActionResult<Book>> GetBookByBookIdAsync(int bookId)
        {
            Book dbBook = await _repository.GetBookAsync(bookId);
            
            if (dbBook == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Book>(dbBook));
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Publishers/{pubId}/Books")]
        public async Task<ActionResult<Book[]>> GetBooksByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "BookId Desc")
        {
            EntityCollection<Book> dbBooks = null;
            try
            {
                dbBooks = await _repository.GetBooksByPubIdAsync(pubId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBooks == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Book> Books = new ModelObjectCollection<Data.Models.Book>
            {
                TotalCount = dbBooks.TotalCount,
                PageNumber = dbBooks.PageNumber,
                PageSize = dbBooks.PageSize,
                TotalPages = dbBooks.TotalPages,
                SortBy = dbBooks.SortBy,
                NextPageNumber = dbBooks.NextPageNumber,
                PrevPageNumber = dbBooks.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Book []>(dbBooks.Data)
            };
            
            Books.NextPageUrl = (Books.PageNumber == Books.TotalPages) ? "" : ("api/Books?pageNumber" + Books.NextPageNumber.ToString())
                + "&pageSize=" + Books.PageSize.ToString()
                + "&sortBy=" + Books.SortBy;
            Books.PrevPageUrl = (Books.PageNumber == 1) ? "" : ("api/Books?pageNumber" + Books.PrevPageNumber.ToString())
                + "&pageSize=" + Books.PageSize.ToString()
                + "&sortBy=" + Books.SortBy;
            
            return Ok(Books);
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Books")]
        public async Task<ActionResult<Data.Models.Book>> CreateNewBook(Data.Models.BookForCreate newBook)
        {
            Data.Entities.Book dbNewBook = null;
            try
            {
                dbNewBook = _mapper.Map<Data.Entities.Book>(newBook);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewBook == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Book>(dbNewBook);
            await _repository.SaveChangesAsync();
            
            Data.Models.Book addedBook = _mapper.Map<Data.Models.Book>(dbNewBook);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetBookByBookId", "Books",  addedBook);
            
            return this.Created(url, addedBook);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Books/{bookId}")]
        public async Task<ActionResult<Data.Models.Book>> UpdateBook(int bookId, Data.Models.BookForUpdate updatedBook)
        {
            try
            {
                Data.Entities.Book dbBook = await _repository.GetBookAsync(bookId);
                
                if (dbBook == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedBook, dbBook);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Book savedBook = _mapper.Map<Data.Models.Book>(dbBook);
                    return Ok(savedBook);            
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
        [Route("api/Books/{bookId}")]
        public async Task<ActionResult<Data.Models.Book>> PatchBook(int bookId, JsonPatchDocument<Data.Models.BookForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Book dbBook = await _repository.GetBookAsync(bookId);
                if (dbBook == null)
                {
                    return NotFound();
                }
                
                var updatedBook = _mapper.Map<Data.Models.BookForUpdate>(dbBook);
                patchDocument.ApplyTo(updatedBook, ModelState);
                
                _mapper.Map(updatedBook, dbBook);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Book savedBook = _mapper.Map<Data.Models.Book>(await _repository.GetBookAsync(bookId));
                    return Ok(savedBook);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch book " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Books/{bookId}")]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            try
            {
                Data.Entities.Book dbBook = await _repository.GetBookAsync(bookId);
                if (dbBook == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Book>(dbBook);
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
