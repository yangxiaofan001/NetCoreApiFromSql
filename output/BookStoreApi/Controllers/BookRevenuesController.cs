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
    public class BookRevenuesController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookRevenuesController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public BookRevenuesController(IBookStoreApiRepository repository, IMapper mapper, ILogger<BookRevenuesController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllBookRevenues
        [HttpGet]
        [Route("api/BookRevenues")]
        public async Task<ActionResult<BookRevenue[]>> GetAllBookRevenues(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "")
        {
            EntityCollection<BookRevenue> dbBookRevenues= null;
            try
            {
                dbBookRevenues= await _repository.GetAllBookRevenuesAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbBookRevenues == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.BookRevenue> BookRevenues = new ModelObjectCollection<Data.Models.BookRevenue>
            {
                TotalCount = dbBookRevenues.TotalCount,
                PageNumber = dbBookRevenues.PageNumber,
                PageSize = dbBookRevenues.PageSize,
                TotalPages = dbBookRevenues.TotalPages,
                SortBy = dbBookRevenues.SortBy,
                NextPageNumber = dbBookRevenues.NextPageNumber,
                PrevPageNumber = dbBookRevenues.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.BookRevenue []>(dbBookRevenues.Data)
            };
            
            BookRevenues.NextPageUrl = (BookRevenues.PageNumber == BookRevenues.TotalPages) ? "" : ("api/BookRevenues?pageNumber" + BookRevenues.NextPageNumber.ToString())
                +"&pageSize=" + BookRevenues.PageSize.ToString()
                +"&sortBy=" + BookRevenues.SortBy;
            BookRevenues.PrevPageUrl = (BookRevenues.PageNumber == 1) ? "" : ("api/BookRevenues?pageNumber" + BookRevenues.PrevPageNumber.ToString())
                +"&pageSize=" + BookRevenues.PageSize.ToString()
                +"&sortBy=" + BookRevenues.SortBy;
            
            return Ok(BookRevenues);
        }
        
    }
}
