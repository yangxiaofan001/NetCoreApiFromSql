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
    public class SalesController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SalesController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public SalesController(IBookStoreApiRepository repository, IMapper mapper, ILogger<SalesController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllSales
        [HttpGet]
        [Route("api/Sales")]
        public async Task<ActionResult<Sale[]>> GetAllSales(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            EntityCollection<Sale> dbSales= null;
            try
            {
                dbSales= await _repository.GetAllSalesAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbSales == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Sale> Sales = new ModelObjectCollection<Data.Models.Sale>
            {
                TotalCount = dbSales.TotalCount,
                PageNumber = dbSales.PageNumber,
                PageSize = dbSales.PageSize,
                TotalPages = dbSales.TotalPages,
                SortBy = dbSales.SortBy,
                NextPageNumber = dbSales.NextPageNumber,
                PrevPageNumber = dbSales.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Sale []>(dbSales.Data)
            };
            
            Sales.NextPageUrl = (Sales.PageNumber == Sales.TotalPages) ? "" : ("api/Sales?pageNumber" + Sales.NextPageNumber.ToString())
                +"&pageSize=" + Sales.PageSize.ToString()
                +"&sortBy=" + Sales.SortBy;
            Sales.PrevPageUrl = (Sales.PageNumber == 1) ? "" : ("api/Sales?pageNumber" + Sales.PrevPageNumber.ToString())
                +"&pageSize=" + Sales.PageSize.ToString()
                +"&sortBy=" + Sales.SortBy;
            
            return Ok(Sales);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Sales/{saleId}")]
        public async Task<ActionResult<Sale>> GetSaleBySaleIdAsync(int saleId)
        {
            Sale dbSale = await _repository.GetSaleAsync(saleId);
            
            if (dbSale == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Sale>(dbSale));
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Books/{bookId}/Sales")]
        public async Task<ActionResult<Sale[]>> GetSalesByBookIdAsync(int bookId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            EntityCollection<Sale> dbSales = null;
            try
            {
                dbSales = await _repository.GetSalesByBookIdAsync(bookId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbSales == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Sale> Sales = new ModelObjectCollection<Data.Models.Sale>
            {
                TotalCount = dbSales.TotalCount,
                PageNumber = dbSales.PageNumber,
                PageSize = dbSales.PageSize,
                TotalPages = dbSales.TotalPages,
                SortBy = dbSales.SortBy,
                NextPageNumber = dbSales.NextPageNumber,
                PrevPageNumber = dbSales.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Sale []>(dbSales.Data)
            };
            
            Sales.NextPageUrl = (Sales.PageNumber == Sales.TotalPages) ? "" : ("api/Sales?pageNumber" + Sales.NextPageNumber.ToString())
                + "&pageSize=" + Sales.PageSize.ToString()
                + "&sortBy=" + Sales.SortBy;
            Sales.PrevPageUrl = (Sales.PageNumber == 1) ? "" : ("api/Sales?pageNumber" + Sales.PrevPageNumber.ToString())
                + "&pageSize=" + Sales.PageSize.ToString()
                + "&sortBy=" + Sales.SortBy;
            
            return Ok(Sales);
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Stores/{storeId}/Sales")]
        public async Task<ActionResult<Sale[]>> GetSalesByStoreIdAsync(string storeId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "SaleId Desc")
        {
            EntityCollection<Sale> dbSales = null;
            try
            {
                dbSales = await _repository.GetSalesByStoreIdAsync(storeId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbSales == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Sale> Sales = new ModelObjectCollection<Data.Models.Sale>
            {
                TotalCount = dbSales.TotalCount,
                PageNumber = dbSales.PageNumber,
                PageSize = dbSales.PageSize,
                TotalPages = dbSales.TotalPages,
                SortBy = dbSales.SortBy,
                NextPageNumber = dbSales.NextPageNumber,
                PrevPageNumber = dbSales.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Sale []>(dbSales.Data)
            };
            
            Sales.NextPageUrl = (Sales.PageNumber == Sales.TotalPages) ? "" : ("api/Sales?pageNumber" + Sales.NextPageNumber.ToString())
                + "&pageSize=" + Sales.PageSize.ToString()
                + "&sortBy=" + Sales.SortBy;
            Sales.PrevPageUrl = (Sales.PageNumber == 1) ? "" : ("api/Sales?pageNumber" + Sales.PrevPageNumber.ToString())
                + "&pageSize=" + Sales.PageSize.ToString()
                + "&sortBy=" + Sales.SortBy;
            
            return Ok(Sales);
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Sales")]
        public async Task<ActionResult<Data.Models.Sale>> CreateNewSale(Data.Models.SaleForCreate newSale)
        {
            Data.Entities.Sale dbNewSale = null;
            try
            {
                dbNewSale = _mapper.Map<Data.Entities.Sale>(newSale);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewSale == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Sale>(dbNewSale);
            await _repository.SaveChangesAsync();
            
            Data.Models.Sale addedSale = _mapper.Map<Data.Models.Sale>(dbNewSale);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetSaleBySaleId", "Sales",  addedSale);
            
            return this.Created(url, addedSale);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Sales/{saleId}")]
        public async Task<ActionResult<Data.Models.Sale>> UpdateSale(int saleId, Data.Models.SaleForUpdate updatedSale)
        {
            try
            {
                Data.Entities.Sale dbSale = await _repository.GetSaleAsync(saleId);
                
                if (dbSale == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedSale, dbSale);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Sale savedSale = _mapper.Map<Data.Models.Sale>(dbSale);
                    return Ok(savedSale);            
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
        [Route("api/Sales/{saleId}")]
        public async Task<ActionResult<Data.Models.Sale>> PatchSale(int saleId, JsonPatchDocument<Data.Models.SaleForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Sale dbSale = await _repository.GetSaleAsync(saleId);
                if (dbSale == null)
                {
                    return NotFound();
                }
                
                var updatedSale = _mapper.Map<Data.Models.SaleForUpdate>(dbSale);
                patchDocument.ApplyTo(updatedSale, ModelState);
                
                _mapper.Map(updatedSale, dbSale);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Sale savedSale = _mapper.Map<Data.Models.Sale>(await _repository.GetSaleAsync(saleId));
                    return Ok(savedSale);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch sale " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Sales/{saleId}")]
        public async Task<IActionResult> DeleteSale(int saleId)
        {
            try
            {
                Data.Entities.Sale dbSale = await _repository.GetSaleAsync(saleId);
                if (dbSale == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Sale>(dbSale);
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
