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
    public class MuscleGroupsController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<MuscleGroupsController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public MuscleGroupsController(IBookStoreApiRepository repository, IMapper mapper, ILogger<MuscleGroupsController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllMuscleGroups
        [HttpGet]
        [Route("api/MuscleGroups")]
        public async Task<ActionResult<MuscleGroup[]>> GetAllMuscleGroups(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "MuscleGroupId Desc")
        {
            EntityCollection<MuscleGroup> dbMuscleGroups= null;
            try
            {
                dbMuscleGroups= await _repository.GetAllMuscleGroupsAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbMuscleGroups == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.MuscleGroup> MuscleGroups = new ModelObjectCollection<Data.Models.MuscleGroup>
            {
                TotalCount = dbMuscleGroups.TotalCount,
                PageNumber = dbMuscleGroups.PageNumber,
                PageSize = dbMuscleGroups.PageSize,
                TotalPages = dbMuscleGroups.TotalPages,
                SortBy = dbMuscleGroups.SortBy,
                NextPageNumber = dbMuscleGroups.NextPageNumber,
                PrevPageNumber = dbMuscleGroups.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.MuscleGroup []>(dbMuscleGroups.Data)
            };
            
            MuscleGroups.NextPageUrl = (MuscleGroups.PageNumber == MuscleGroups.TotalPages) ? "" : ("api/MuscleGroups?pageNumber" + MuscleGroups.NextPageNumber.ToString())
                +"&pageSize=" + MuscleGroups.PageSize.ToString()
                +"&sortBy=" + MuscleGroups.SortBy;
            MuscleGroups.PrevPageUrl = (MuscleGroups.PageNumber == 1) ? "" : ("api/MuscleGroups?pageNumber" + MuscleGroups.PrevPageNumber.ToString())
                +"&pageSize=" + MuscleGroups.PageSize.ToString()
                +"&sortBy=" + MuscleGroups.SortBy;
            
            return Ok(MuscleGroups);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/MuscleGroups/{muscleGroupId}")]
        public async Task<ActionResult<MuscleGroup>> GetMuscleGroupByMuscleGroupIdAsync(int muscleGroupId)
        {
            MuscleGroup dbMuscleGroup = await _repository.GetMuscleGroupAsync(muscleGroupId);
            
            if (dbMuscleGroup == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.MuscleGroup>(dbMuscleGroup));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/MuscleGroups")]
        public async Task<ActionResult<Data.Models.MuscleGroup>> CreateNewMuscleGroup(Data.Models.MuscleGroupForCreate newMuscleGroup)
        {
            Data.Entities.MuscleGroup dbNewMuscleGroup = null;
            try
            {
                dbNewMuscleGroup = _mapper.Map<Data.Entities.MuscleGroup>(newMuscleGroup);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewMuscleGroup == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.MuscleGroup>(dbNewMuscleGroup);
            await _repository.SaveChangesAsync();
            
            Data.Models.MuscleGroup addedMuscleGroup = _mapper.Map<Data.Models.MuscleGroup>(dbNewMuscleGroup);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetMuscleGroupByMuscleGroupId", "MuscleGroups",  addedMuscleGroup);
            
            return this.Created(url, addedMuscleGroup);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/MuscleGroups/{muscleGroupId}")]
        public async Task<ActionResult<Data.Models.MuscleGroup>> UpdateMuscleGroup(int muscleGroupId, Data.Models.MuscleGroupForUpdate updatedMuscleGroup)
        {
            try
            {
                Data.Entities.MuscleGroup dbMuscleGroup = await _repository.GetMuscleGroupAsync(muscleGroupId);
                
                if (dbMuscleGroup == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedMuscleGroup, dbMuscleGroup);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.MuscleGroup savedMuscleGroup = _mapper.Map<Data.Models.MuscleGroup>(dbMuscleGroup);
                    return Ok(savedMuscleGroup);            
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
        [Route("api/MuscleGroups/{muscleGroupId}")]
        public async Task<ActionResult<Data.Models.MuscleGroup>> PatchMuscleGroup(int muscleGroupId, JsonPatchDocument<Data.Models.MuscleGroupForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.MuscleGroup dbMuscleGroup = await _repository.GetMuscleGroupAsync(muscleGroupId);
                if (dbMuscleGroup == null)
                {
                    return NotFound();
                }
                
                var updatedMuscleGroup = _mapper.Map<Data.Models.MuscleGroupForUpdate>(dbMuscleGroup);
                patchDocument.ApplyTo(updatedMuscleGroup, ModelState);
                
                _mapper.Map(updatedMuscleGroup, dbMuscleGroup);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.MuscleGroup savedMuscleGroup = _mapper.Map<Data.Models.MuscleGroup>(await _repository.GetMuscleGroupAsync(muscleGroupId));
                    return Ok(savedMuscleGroup);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch muscleGroup " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/MuscleGroups/{muscleGroupId}")]
        public async Task<IActionResult> DeleteMuscleGroup(int muscleGroupId)
        {
            try
            {
                Data.Entities.MuscleGroup dbMuscleGroup = await _repository.GetMuscleGroupAsync(muscleGroupId);
                if (dbMuscleGroup == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.MuscleGroup>(dbMuscleGroup);
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
