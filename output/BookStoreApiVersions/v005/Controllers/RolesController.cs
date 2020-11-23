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
    public class RolesController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<RolesController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public RolesController(IBookStoreApiRepository repository, IMapper mapper, ILogger<RolesController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllRoles
        [HttpGet]
        [Route("api/Roles")]
        public async Task<ActionResult<Role[]>> GetAllRoles(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "RoleId Desc")
        {
            EntityCollection<Role> dbRoles= null;
            try
            {
                dbRoles= await _repository.GetAllRolesAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbRoles == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Role> Roles = new ModelObjectCollection<Data.Models.Role>
            {
                TotalCount = dbRoles.TotalCount,
                PageNumber = dbRoles.PageNumber,
                PageSize = dbRoles.PageSize,
                TotalPages = dbRoles.TotalPages,
                SortBy = dbRoles.SortBy,
                NextPageNumber = dbRoles.NextPageNumber,
                PrevPageNumber = dbRoles.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Role []>(dbRoles.Data)
            };
            
            Roles.NextPageUrl = (Roles.PageNumber == Roles.TotalPages) ? "" : ("api/Roles?pageNumber" + Roles.NextPageNumber.ToString())
                +"&pageSize=" + Roles.PageSize.ToString()
                +"&sortBy=" + Roles.SortBy;
            Roles.PrevPageUrl = (Roles.PageNumber == 1) ? "" : ("api/Roles?pageNumber" + Roles.PrevPageNumber.ToString())
                +"&pageSize=" + Roles.PageSize.ToString()
                +"&sortBy=" + Roles.SortBy;
            
            return Ok(Roles);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Roles/{roleId}")]
        public async Task<ActionResult<Role>> GetRoleByRoleIdAsync(short roleId)
        {
            Role dbRole = await _repository.GetRoleAsync(roleId);
            
            if (dbRole == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Role>(dbRole));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Roles")]
        public async Task<ActionResult<Data.Models.Role>> CreateNewRole(Data.Models.RoleForCreate newRole)
        {
            Data.Entities.Role dbNewRole = null;
            try
            {
                dbNewRole = _mapper.Map<Data.Entities.Role>(newRole);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewRole == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Role>(dbNewRole);
            await _repository.SaveChangesAsync();
            
            Data.Models.Role addedRole = _mapper.Map<Data.Models.Role>(dbNewRole);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetRoleByRoleId", "Roles",  addedRole);
            
            return this.Created(url, addedRole);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Roles/{roleId}")]
        public async Task<ActionResult<Data.Models.Role>> UpdateRole(short roleId, Data.Models.RoleForUpdate updatedRole)
        {
            try
            {
                Data.Entities.Role dbRole = await _repository.GetRoleAsync(roleId);
                
                if (dbRole == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedRole, dbRole);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Role savedRole = _mapper.Map<Data.Models.Role>(dbRole);
                    return Ok(savedRole);            
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
        [Route("api/Roles/{roleId}")]
        public async Task<ActionResult<Data.Models.Role>> PatchRole(short roleId, JsonPatchDocument<Data.Models.RoleForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Role dbRole = await _repository.GetRoleAsync(roleId);
                if (dbRole == null)
                {
                    return NotFound();
                }
                
                var updatedRole = _mapper.Map<Data.Models.RoleForUpdate>(dbRole);
                patchDocument.ApplyTo(updatedRole, ModelState);
                
                _mapper.Map(updatedRole, dbRole);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Role savedRole = _mapper.Map<Data.Models.Role>(await _repository.GetRoleAsync(roleId));
                    return Ok(savedRole);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch role " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Roles/{roleId}")]
        public async Task<IActionResult> DeleteRole(short roleId)
        {
            try
            {
                Data.Entities.Role dbRole = await _repository.GetRoleAsync(roleId);
                if (dbRole == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Role>(dbRole);
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
