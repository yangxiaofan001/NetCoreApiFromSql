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
    public class JobsController : ControllerBase
    {
        private readonly IBookStoreApiRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<JobsController> _logger;
        private readonly LinkGenerator _linkgenerator;
        public JobsController(IBookStoreApiRepository repository, IMapper mapper, ILogger<JobsController> logger, LinkGenerator linkgenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _linkgenerator = linkgenerator;
        }
        
        // GetAllJobs
        [HttpGet]
        [Route("api/Jobs")]
        public async Task<ActionResult<Job[]>> GetAllJobs(int pageNumber = 1, int pageSize = Data.Constants.Paging.DefaultPageSize, string sortBy = "JobId Desc")
        {
            EntityCollection<Job> dbJobs= null;
            try
            {
                dbJobs= await _repository.GetAllJobsAsync(pageNumber, pageSize, sortBy);
            }
            catch (ParseException ex)
            {
                return BadRequest("Request format is invalid: " + ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
            
            if (dbJobs == null)
            {
                return NotFound();
            }
            
            Data.ModelObjectCollection<Data.Models.Job> Jobs = new ModelObjectCollection<Data.Models.Job>
            {
                TotalCount = dbJobs.TotalCount,
                PageNumber = dbJobs.PageNumber,
                PageSize = dbJobs.PageSize,
                TotalPages = dbJobs.TotalPages,
                SortBy = dbJobs.SortBy,
                NextPageNumber = dbJobs.NextPageNumber,
                PrevPageNumber = dbJobs.PrevPageNumber,
                NextPageUrl = "",
                PrevPageUrl = "",
                Data = _mapper.Map<Data.Models.Job []>(dbJobs.Data)
            };
            
            Jobs.NextPageUrl = (Jobs.PageNumber == Jobs.TotalPages) ? "" : ("api/Jobs?pageNumber" + Jobs.NextPageNumber.ToString())
                +"&pageSize=" + Jobs.PageSize.ToString()
                +"&sortBy=" + Jobs.SortBy;
            Jobs.PrevPageUrl = (Jobs.PageNumber == 1) ? "" : ("api/Jobs?pageNumber" + Jobs.PrevPageNumber.ToString())
                +"&pageSize=" + Jobs.PageSize.ToString()
                +"&sortBy=" + Jobs.SortBy;
            
            return Ok(Jobs);
        }
        
        // GetByPk
        [HttpGet]
        [Route("api/Jobs/{jobId}")]
        public async Task<ActionResult<Job>> GetJobByJobIdAsync(short jobId)
        {
            Job dbJob = await _repository.GetJobAsync(jobId);
            
            if (dbJob == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<Data.Models.Job>(dbJob));
        }
        
        // HttpPost create new
        [HttpPost]
        [Route("api/Jobs")]
        public async Task<ActionResult<Data.Models.Job>> CreateNewJob(Data.Models.JobForCreate newJob)
        {
            Data.Entities.Job dbNewJob = null;
            try
            {
                dbNewJob = _mapper.Map<Data.Entities.Job>(newJob);
            }
            catch(Exception ex)
            {
                return BadRequest("Input is in invalid format: " + ex.Message);
            }
            
            if (dbNewJob == null)
            {
                return BadRequest("Input is in invalid format");
            }
            
            await _repository.AddAsync<Data.Entities.Job>(dbNewJob);
            await _repository.SaveChangesAsync();
            
            Data.Models.Job addedJob = _mapper.Map<Data.Models.Job>(dbNewJob);
            
            var url = _linkgenerator.GetPathByAction(HttpContext, "GetJobByJobId", "Jobs",  addedJob);
            
            return this.Created(url, addedJob);
        }
        
        // HttpPut full update
        [HttpPut]
        [Route("api/Jobs/{jobId}")]
        public async Task<ActionResult<Data.Models.Job>> UpdateJob(short jobId, Data.Models.JobForUpdate updatedJob)
        {
            try
            {
                Data.Entities.Job dbJob = await _repository.GetJobAsync(jobId);
                
                if (dbJob == null)
                {
                    return NotFound();
                }
                
                _mapper.Map(updatedJob, dbJob);
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Job savedJob = _mapper.Map<Data.Models.Job>(dbJob);
                    return Ok(savedJob);            
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
        [Route("api/Jobs/{jobId}")]
        public async Task<ActionResult<Data.Models.Job>> PatchJob(short jobId, JsonPatchDocument<Data.Models.JobForUpdate> patchDocument)
        {
            try
            {
                Data.Entities.Job dbJob = await _repository.GetJobAsync(jobId);
                if (dbJob == null)
                {
                    return NotFound();
                }
                
                var updatedJob = _mapper.Map<Data.Models.JobForUpdate>(dbJob);
                patchDocument.ApplyTo(updatedJob, ModelState);
                
                _mapper.Map(updatedJob, dbJob);
                
                if (await _repository.SaveChangesAsync())
                {
                    Data.Models.Job savedJob = _mapper.Map<Data.Models.Job>(await _repository.GetJobAsync(jobId));
                    return Ok(savedJob);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to save to database");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to patch job " + ex.Message);
            }
        }
        
        [HttpDelete]
        [Route("api/Jobs/{jobId}")]
        public async Task<IActionResult> DeleteJob(short jobId)
        {
            try
            {
                Data.Entities.Job dbJob = await _repository.GetJobAsync(jobId);
                if (dbJob == null)
                {
                    return NotFound();
                }
                
                _repository.Delete<Data.Entities.Job>(dbJob);
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
