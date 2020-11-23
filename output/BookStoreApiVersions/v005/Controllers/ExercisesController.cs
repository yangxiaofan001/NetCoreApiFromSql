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
    public class ExercisesController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ExercisesController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public ExercisesController(IBookStoreApiRepository repository, IMapper mapper, ILogger<ExercisesController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllExercises
        [HttpGet]
        [Route("api/Exercises")]
        public async Task<ActionResult<Exercise[]>> GetAllExercises(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "ExerciseId Desc")
        {
            EntityCollection<Exercise> dbExercises= null;
            try
            {
                dbExercises= await _repository.GetAllExercisesAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbExercises == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Exercise> Exercises = new ModelObjectCollection<Data.Models.Exercise>
            {
                TotalCount = dbExercises.TotalCount,
                PageNumber = dbExercises.PageNumber,
                PageSize = dbExercises.PageSize,
                TotalPages = dbExercises.TotalPages,
                SortBy = dbExercises.SortBy,
                NextPageNumber = dbExercises.NextPageNumber,
                PrevPageNumber = dbExercises.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Exercise []>(dbExercises.Data)
            };
            
            Exercises.NextPageUrl = (Exercises.PageNumber == Exercises.TotalPages) ? "" : ("api/Exercises?pageNumber" + Exercises.NextPageNumber.ToString())
                +"&pageSize=" + Exercises.PageSize.ToString()
                +"&sortBy=" + Exercises.SortBy;
            Exercises.PrevPageUrl = (Exercises.PageNumber == 1) ? "" : ("api/Exercises?pageNumber" + Exercises.PrevPageNumber.ToString())
                +"&pageSize=" + Exercises.PageSize.ToString()
                +"&sortBy=" + Exercises.SortBy;
            
            return Ok(Exercises);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Exercises/{exerciseId}")]
        public async Task<ActionResult<Exercise>> GetExerciseByExerciseIdAsync(int exerciseId)
        {
            Exercise dbExercise = await _repository.GetExerciseAsync(exerciseId);
            
            if (dbExercise == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Exercise>(dbExercise));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Exercises")]
        public async Task<ActionResult<Data.Models.Exercise>> CreateNewExercise(Data.Models.ExerciseForCreate newExercise)
        {
            Data.Entities.Exercise dbNewExercise = null;
            try
            {
                dbNewExercise = _mapper.Map<Data.Entities.Exercise>(newExercise);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewExercise == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Exercise>(dbNewExercise);
            await _repository.SaveChangesAsync();
            
            Data.Models.Exercise addedExercise = _mapper.Map<Data.Models.Exercise>(dbNewExercise);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetExerciseByExerciseId", "Exercises",  addedExercise);
            
            return this.Created(url, addedExercise);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Exercises/{exerciseId}")]
        public async Task<ActionResult<Data.Models.Exercise>> UpdateExercise(int exerciseId, Data.Models.ExerciseForUpdate updatedExercise)
        {
            try
            {
                Data.Entities.Exercise dbExercise = await _repository.GetExerciseAsync(exerciseId);
                
                if (dbExercise == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedExercise, dbExercise);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Exercise savedExercise = _mapper.Map<Data.Models.Exercise>(dbExercise);
                    return Ok(savedExercise);            
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
        [Route("api/Exercises/{exerciseId}")]
        public async Task<ActionResult<Data.Models.Exercise>> PatchExercise(int exerciseId, JsonPatchDocument<Data.Models.ExerciseForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Exercise dbExercise = await _repository.GetExerciseAsync(exerciseId);
                if (dbExercise == null)
                {
                    return NotFound();
                }
                
                var updatedExercise = _mapper.Map<Data.Models.ExerciseForUpdate>(dbExercise);
                patchDocument.ApplyTo(updatedExercise, ModelState);
                
                _mapper.Map(updatedExercise, dbExercise);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Exercise savedExercise = _mapper.Map<Data.Models.Exercise>(await _repository.GetExerciseAsync(exerciseId));
                    return Ok(savedExercise);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch exercise " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Exercises/{exerciseId}")]
        public async Task<IActionResult> DeleteExercise(int exerciseId)
        {
            try
            {
                Data.Entities.Exercise dbExercise = await _repository.GetExerciseAsync(exerciseId);
                if (dbExercise == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Exercise>(dbExercise);
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
