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
    public class AuthorRevenuesController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorRevenuesController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public AuthorRevenuesController(IBookStoreApiRepository repository, IMapper mapper, ILogger<AuthorRevenuesController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllAuthorRevenues
        [HttpGet]
        [Route("api/AuthorRevenues")]
        public async Task<ActionResult<AuthorRevenue[]>> GetAllAuthorRevenues(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "")
        {
            EntityCollection<AuthorRevenue> dbAuthorRevenues= null;
            try
            {
                dbAuthorRevenues= await _repository.GetAllAuthorRevenuesAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbAuthorRevenues == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.AuthorRevenue> AuthorRevenues = new ModelObjectCollection<Data.Models.AuthorRevenue>
            {
                TotalCount = dbAuthorRevenues.TotalCount,
                PageNumber = dbAuthorRevenues.PageNumber,
                PageSize = dbAuthorRevenues.PageSize,
                TotalPages = dbAuthorRevenues.TotalPages,
                SortBy = dbAuthorRevenues.SortBy,
                NextPageNumber = dbAuthorRevenues.NextPageNumber,
                PrevPageNumber = dbAuthorRevenues.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.AuthorRevenue []>(dbAuthorRevenues.Data)
            };
            
            AuthorRevenues.NextPageUrl = (AuthorRevenues.PageNumber == AuthorRevenues.TotalPages) ? "" : ("api/AuthorRevenues?pageNumber" + AuthorRevenues.NextPageNumber.ToString())
                +"&pageSize=" + AuthorRevenues.PageSize.ToString()
                +"&sortBy=" + AuthorRevenues.SortBy;
            AuthorRevenues.PrevPageUrl = (AuthorRevenues.PageNumber == 1) ? "" : ("api/AuthorRevenues?pageNumber" + AuthorRevenues.PrevPageNumber.ToString())
                +"&pageSize=" + AuthorRevenues.PageSize.ToString()
                +"&sortBy=" + AuthorRevenues.SortBy;
            
            return Ok(AuthorRevenues);
        }
        
    }
}
