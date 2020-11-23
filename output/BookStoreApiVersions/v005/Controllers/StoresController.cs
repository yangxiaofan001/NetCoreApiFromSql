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
    public class StoresController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<StoresController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public StoresController(IBookStoreApiRepository repository, IMapper mapper, ILogger<StoresController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllStores
        [HttpGet]
        [Route("api/Stores")]
        public async Task<ActionResult<Store[]>> GetAllStores(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "StoreId Desc")
        {
            EntityCollection<Store> dbStores= null;
            try
            {
                dbStores= await _repository.GetAllStoresAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbStores == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Store> Stores = new ModelObjectCollection<Data.Models.Store>
            {
                TotalCount = dbStores.TotalCount,
                PageNumber = dbStores.PageNumber,
                PageSize = dbStores.PageSize,
                TotalPages = dbStores.TotalPages,
                SortBy = dbStores.SortBy,
                NextPageNumber = dbStores.NextPageNumber,
                PrevPageNumber = dbStores.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Store []>(dbStores.Data)
            };
            
            Stores.NextPageUrl = (Stores.PageNumber == Stores.TotalPages) ? "" : ("api/Stores?pageNumber" + Stores.NextPageNumber.ToString())
                +"&pageSize=" + Stores.PageSize.ToString()
                +"&sortBy=" + Stores.SortBy;
            Stores.PrevPageUrl = (Stores.PageNumber == 1) ? "" : ("api/Stores?pageNumber" + Stores.PrevPageNumber.ToString())
                +"&pageSize=" + Stores.PageSize.ToString()
                +"&sortBy=" + Stores.SortBy;
            
            return Ok(Stores);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Stores/{storeId}")]
        public async Task<ActionResult<Store>> GetStoreByStoreIdAsync(string storeId)
        {
            Store dbStore = await _repository.GetStoreAsync(storeId);
            
            if (dbStore == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Store>(dbStore));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Stores")]
        public async Task<ActionResult<Data.Models.Store>> CreateNewStore(Data.Models.StoreForCreate newStore)
        {
            Data.Entities.Store dbNewStore = null;
            try
            {
                dbNewStore = _mapper.Map<Data.Entities.Store>(newStore);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewStore == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Store>(dbNewStore);
            await _repository.SaveChangesAsync();
            
            Data.Models.Store addedStore = _mapper.Map<Data.Models.Store>(dbNewStore);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetStoreByStoreId", "Stores",  addedStore);
            
            return this.Created(url, addedStore);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Stores/{storeId}")]
        public async Task<ActionResult<Data.Models.Store>> UpdateStore(string storeId, Data.Models.StoreForUpdate updatedStore)
        {
            try
            {
                Data.Entities.Store dbStore = await _repository.GetStoreAsync(storeId);
                
                if (dbStore == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedStore, dbStore);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Store savedStore = _mapper.Map<Data.Models.Store>(dbStore);
                    return Ok(savedStore);            
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
        [Route("api/Stores/{storeId}")]
        public async Task<ActionResult<Data.Models.Store>> PatchStore(string storeId, JsonPatchDocument<Data.Models.StoreForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Store dbStore = await _repository.GetStoreAsync(storeId);
                if (dbStore == null)
                {
                    return NotFound();
                }
                
                var updatedStore = _mapper.Map<Data.Models.StoreForUpdate>(dbStore);
                patchDocument.ApplyTo(updatedStore, ModelState);
                
                _mapper.Map(updatedStore, dbStore);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Store savedStore = _mapper.Map<Data.Models.Store>(await _repository.GetStoreAsync(storeId));
                    return Ok(savedStore);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch store " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Stores/{storeId}")]
        public async Task<IActionResult> DeleteStore(string storeId)
        {
            try
            {
                Data.Entities.Store dbStore = await _repository.GetStoreAsync(storeId);
                if (dbStore == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Store>(dbStore);
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
