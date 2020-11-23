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
    public class UsersController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public UsersController(IBookStoreApiRepository repository, IMapper mapper, ILogger<UsersController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllUsers
        [HttpGet]
        [Route("api/Users")]
        public async Task<ActionResult<User[]>> GetAllUsers(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            EntityCollection<User> dbUsers= null;
            try
            {
                dbUsers= await _repository.GetAllUsersAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbUsers == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.User> Users = new ModelObjectCollection<Data.Models.User>
            {
                TotalCount = dbUsers.TotalCount,
                PageNumber = dbUsers.PageNumber,
                PageSize = dbUsers.PageSize,
                TotalPages = dbUsers.TotalPages,
                SortBy = dbUsers.SortBy,
                NextPageNumber = dbUsers.NextPageNumber,
                PrevPageNumber = dbUsers.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.User []>(dbUsers.Data)
            };
            
            Users.NextPageUrl = (Users.PageNumber == Users.TotalPages) ? "" : ("api/Users?pageNumber" + Users.NextPageNumber.ToString())
                +"&pageSize=" + Users.PageSize.ToString()
                +"&sortBy=" + Users.SortBy;
            Users.PrevPageUrl = (Users.PageNumber == 1) ? "" : ("api/Users?pageNumber" + Users.PrevPageNumber.ToString())
                +"&pageSize=" + Users.PageSize.ToString()
                +"&sortBy=" + Users.SortBy;
            
            return Ok(Users);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Users/{userId}")]
        public async Task<ActionResult<User>> GetUserByUserIdAsync(int userId)
        {
            User dbUser = await _repository.GetUserAsync(userId);
            
            if (dbUser == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.User>(dbUser));
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Roles/{roleId}/Users")]
        public async Task<ActionResult<User[]>> GetUsersByRoleIdAsync(short roleId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            EntityCollection<User> dbUsers = null;
            try
            {
                dbUsers = await _repository.GetUsersByRoleIdAsync(roleId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbUsers == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.User> Users = new ModelObjectCollection<Data.Models.User>
            {
                TotalCount = dbUsers.TotalCount,
                PageNumber = dbUsers.PageNumber,
                PageSize = dbUsers.PageSize,
                TotalPages = dbUsers.TotalPages,
                SortBy = dbUsers.SortBy,
                NextPageNumber = dbUsers.NextPageNumber,
                PrevPageNumber = dbUsers.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.User []>(dbUsers.Data)
            };
            
            Users.NextPageUrl = (Users.PageNumber == Users.TotalPages) ? "" : ("api/Users?pageNumber" + Users.NextPageNumber.ToString())
                + "&pageSize=" + Users.PageSize.ToString()
                + "&sortBy=" + Users.SortBy;
            Users.PrevPageUrl = (Users.PageNumber == 1) ? "" : ("api/Users?pageNumber" + Users.PrevPageNumber.ToString())
                + "&pageSize=" + Users.PageSize.ToString()
                + "&sortBy=" + Users.SortBy;
            
            return Ok(Users);
        }
        
        // GetByFk, where current entity is child
        [HttpGet]
        [Route("api/Publishers/{pubId}/Users")]
        public async Task<ActionResult<User[]>> GetUsersByPubIdAsync(int pubId, int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "UserId Desc")
        {
            EntityCollection<User> dbUsers = null;
            try
            {
                dbUsers = await _repository.GetUsersByPubIdAsync(pubId, pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbUsers == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.User> Users = new ModelObjectCollection<Data.Models.User>
            {
                TotalCount = dbUsers.TotalCount,
                PageNumber = dbUsers.PageNumber,
                PageSize = dbUsers.PageSize,
                TotalPages = dbUsers.TotalPages,
                SortBy = dbUsers.SortBy,
                NextPageNumber = dbUsers.NextPageNumber,
                PrevPageNumber = dbUsers.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.User []>(dbUsers.Data)
            };
            
            Users.NextPageUrl = (Users.PageNumber == Users.TotalPages) ? "" : ("api/Users?pageNumber" + Users.NextPageNumber.ToString())
                + "&pageSize=" + Users.PageSize.ToString()
                + "&sortBy=" + Users.SortBy;
            Users.PrevPageUrl = (Users.PageNumber == 1) ? "" : ("api/Users?pageNumber" + Users.PrevPageNumber.ToString())
                + "&pageSize=" + Users.PageSize.ToString()
                + "&sortBy=" + Users.SortBy;
            
            return Ok(Users);
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Users")]
        public async Task<ActionResult<Data.Models.User>> CreateNewUser(Data.Models.UserForCreate newUser)
        {
            Data.Entities.User dbNewUser = null;
            try
            {
                dbNewUser = _mapper.Map<Data.Entities.User>(newUser);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewUser == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.User>(dbNewUser);
            await _repository.SaveChangesAsync();
            
            Data.Models.User addedUser = _mapper.Map<Data.Models.User>(dbNewUser);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetUserByUserId", "Users",  addedUser);
            
            return this.Created(url, addedUser);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Users/{userId}")]
        public async Task<ActionResult<Data.Models.User>> UpdateUser(int userId, Data.Models.UserForUpdate updatedUser)
        {
            try
            {
                Data.Entities.User dbUser = await _repository.GetUserAsync(userId);
                
                if (dbUser == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedUser, dbUser);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.User savedUser = _mapper.Map<Data.Models.User>(dbUser);
                    return Ok(savedUser);            
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
        [Route("api/Users/{userId}")]
        public async Task<ActionResult<Data.Models.User>> PatchUser(int userId, JsonPatchDocument<Data.Models.UserForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.User dbUser = await _repository.GetUserAsync(userId);
                if (dbUser == null)
                {
                    return NotFound();
                }
                
                var updatedUser = _mapper.Map<Data.Models.UserForUpdate>(dbUser);
                patchDocument.ApplyTo(updatedUser, ModelState);
                
                _mapper.Map(updatedUser, dbUser);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.User savedUser = _mapper.Map<Data.Models.User>(await _repository.GetUserAsync(userId));
                    return Ok(savedUser);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch user " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                Data.Entities.User dbUser = await _repository.GetUserAsync(userId);
                if (dbUser == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.User>(dbUser);
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
