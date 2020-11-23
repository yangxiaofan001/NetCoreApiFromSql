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
    public class RefreshTokensController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<RefreshTokensController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public RefreshTokensController(IBookStoreApiRepository repository, IMapper mapper, ILogger<RefreshTokensController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllRefreshTokens
        [HttpGet]
        [Route("api/RefreshTokens")]
        public async Task<ActionResult<RefreshToken[]>> GetAllRefreshTokens(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc")
        {
            EntityCollection<RefreshToken> dbRefreshTokens= null;
            try
            {
                dbRefreshTokens= await _repository.GetAllRefreshTokensAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbRefreshTokens == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.RefreshToken> RefreshTokens = new ModelObjectCollection<Data.Models.RefreshToken>
            {
                TotalCount = dbRefreshTokens.TotalCount,
                PageNumber = dbRefreshTokens.PageNumber,
                PageSize = dbRefreshTokens.PageSize,
                TotalPages = dbRefreshTokens.TotalPages,
                SortBy = dbRefreshTokens.SortBy,
                NextPageNumber = dbRefreshTokens.NextPageNumber,
                PrevPageNumber = dbRefreshTokens.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.RefreshToken []>(dbRefreshTokens.Data)
            };
            
            RefreshTokens.NextPageUrl = (RefreshTokens.PageNumber == RefreshTokens.TotalPages) ? "" : ("api/RefreshTokens?pageNumber" + RefreshTokens.NextPageNumber.ToString())
                +"&pageSize=" + RefreshTokens.PageSize.ToString()
                +"&sortBy=" + RefreshTokens.SortBy;
            RefreshTokens.PrevPageUrl = (RefreshTokens.PageNumber == 1) ? "" : ("api/RefreshTokens?pageNumber" + RefreshTokens.PrevPageNumber.ToString())
                +"&pageSize=" + RefreshTokens.PageSize.ToString()
                +"&sortBy=" + RefreshTokens.SortBy;
            
            return Ok(RefreshTokens);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/RefreshTokens/{tokenId}")]
        public async Task<ActionResult<RefreshToken>> GetRefreshTokenByTokenIdAsync(int tokenId)
        {
            RefreshToken dbRefreshToken = await _repository.GetRefreshTokenAsync(tokenId);
            
            if (dbRefreshToken == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.RefreshToken>(dbRefreshToken));
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Users/{userId}/RefreshTokens")]
        public async Task<ActionResult<RefreshToken[]>> GetRefreshTokensByUserIdAsync(int userId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "TokenId Desc")
        {
            EntityCollection<RefreshToken> dbRefreshTokens = null;
            try
            {
                dbRefreshTokens = await _repository.GetRefreshTokensByUserIdAsync(userId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbRefreshTokens == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.RefreshToken> RefreshTokens = new ModelObjectCollection<Data.Models.RefreshToken>
            {
                TotalCount = dbRefreshTokens.TotalCount,
                PageNumber = dbRefreshTokens.PageNumber,
                PageSize = dbRefreshTokens.PageSize,
                TotalPages = dbRefreshTokens.TotalPages,
                SortBy = dbRefreshTokens.SortBy,
                NextPageNumber = dbRefreshTokens.NextPageNumber,
                PrevPageNumber = dbRefreshTokens.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.RefreshToken []>(dbRefreshTokens.Data)
            };
            
            RefreshTokens.NextPageUrl = (RefreshTokens.PageNumber == RefreshTokens.TotalPages) ? "" : ("api/RefreshTokens?pageNumber" + RefreshTokens.NextPageNumber.ToString())
                + "&pageSize=" + RefreshTokens.PageSize.ToString()
                + "&sortBy=" + RefreshTokens.SortBy;
            RefreshTokens.PrevPageUrl = (RefreshTokens.PageNumber == 1) ? "" : ("api/RefreshTokens?pageNumber" + RefreshTokens.PrevPageNumber.ToString())
                + "&pageSize=" + RefreshTokens.PageSize.ToString()
                + "&sortBy=" + RefreshTokens.SortBy;
            
            return Ok(RefreshTokens);
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/RefreshTokens")]
        public async Task<ActionResult<Data.Models.RefreshToken>> CreateNewRefreshToken(Data.Models.RefreshTokenForCreate newRefreshToken)
        {
            Data.Entities.RefreshToken dbNewRefreshToken = null;
            try
            {
                dbNewRefreshToken = _mapper.Map<Data.Entities.RefreshToken>(newRefreshToken);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewRefreshToken == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.RefreshToken>(dbNewRefreshToken);
            await _repository.SaveChangesAsync();
            
            Data.Models.RefreshToken addedRefreshToken = _mapper.Map<Data.Models.RefreshToken>(dbNewRefreshToken);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetRefreshTokenByTokenId", "RefreshTokens",  addedRefreshToken);
            
            return this.Created(url, addedRefreshToken);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/RefreshTokens/{tokenId}")]
        public async Task<ActionResult<Data.Models.RefreshToken>> UpdateRefreshToken(int tokenId, Data.Models.RefreshTokenForUpdate updatedRefreshToken)
        {
            try
            {
                Data.Entities.RefreshToken dbRefreshToken = await _repository.GetRefreshTokenAsync(tokenId);
                
                if (dbRefreshToken == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedRefreshToken, dbRefreshToken);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.RefreshToken savedRefreshToken = _mapper.Map<Data.Models.RefreshToken>(dbRefreshToken);
                    return Ok(savedRefreshToken);            
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
        [Route("api/RefreshTokens/{tokenId}")]
        public async Task<ActionResult<Data.Models.RefreshToken>> PatchRefreshToken(int tokenId, JsonPatchDocument<Data.Models.RefreshTokenForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.RefreshToken dbRefreshToken = await _repository.GetRefreshTokenAsync(tokenId);
                if (dbRefreshToken == null)
                {
                    return NotFound();
                }
                
                var updatedRefreshToken = _mapper.Map<Data.Models.RefreshTokenForUpdate>(dbRefreshToken);
                patchDocument.ApplyTo(updatedRefreshToken, ModelState);
                
                _mapper.Map(updatedRefreshToken, dbRefreshToken);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.RefreshToken savedRefreshToken = _mapper.Map<Data.Models.RefreshToken>(await _repository.GetRefreshTokenAsync(tokenId));
                    return Ok(savedRefreshToken);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch refreshToken " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/RefreshTokens/{tokenId}")]
        public async Task<IActionResult> DeleteRefreshToken(int tokenId)
        {
            try
            {
                Data.Entities.RefreshToken dbRefreshToken = await _repository.GetRefreshTokenAsync(tokenId);
                if (dbRefreshToken == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.RefreshToken>(dbRefreshToken);
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
