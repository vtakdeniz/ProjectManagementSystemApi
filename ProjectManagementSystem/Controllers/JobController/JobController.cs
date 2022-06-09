using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.JobDto;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Controllers.JobController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class JobController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public JobController(ManagementContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("{id}", Name = "GetJob")]
        public async Task<ActionResult<ReadJobDto>> GetJob(int id) {
            var user = await GetIdentityUser();

            var job = await _context.jobs
                .Include(job => job.section)
                .Include(job => job.activityHistories)
                .Include(job => job.tags)
                .Include(job => job.checkLists)
                .Include(job => job.jobHasUsers)
                .ThenInclude(rel => rel.user)

                .FirstOrDefaultAsync(job => job.Id == id);

            if (job == null) {
                return NotFound();
            }

            if (job.receiverUserId == user.Id) {
                return Ok(_mapper.Map<ReadJobDto>(job));
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.project_id
                    && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel => rel.project_id == job.project_id
                    && rel.receiver_id == user.Id)
                ||
                await _context.boardHasUsers.AnyAsync(rel => rel.board_id == job.section.board_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == job.section.board_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ReadJobDto>(job));
        }

        [HttpGet("date")]
        public async Task<ActionResult> GetJobsByDate([FromQuery] string timeunit, [FromQuery] int span)
        {
            int seconds = 0;

            if (timeunit.CompareTo("month") == 0)
            {
                seconds = 43200;
            }
            else if (timeunit.CompareTo("week") == 0)
            {
                seconds = 10080;
            }
            else
            {
                seconds = 1440;
            }

            var user = await GetIdentityUser();

            var userBoardJobs = await _context.taskHasUsers
               .Where(rel => rel.user_id == user.Id)
               .Select(rel => rel.job_id)
               .ToListAsync();


            var jobs = await _context.jobs
                .Where(job =>
                (userBoardJobs.Contains(job.Id) || job.receiverUserId == user.Id)
                )
                .ToListAsync();

            var dueJobs = new List<Job>();
            var nowMinutes = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes;
            foreach (Job j in jobs)
            {
                {
                    var boardMinutes = TimeSpan.FromTicks(j.endDate.Ticks).TotalMinutes;
                    var result = (boardMinutes - nowMinutes) / seconds;
                    if (result < span && j.endDate.Date>DateTime.Now)
                    {
                        dueJobs.Add(j);
                    }
                }
            }
            return Ok(_mapper.Map<List<ReadJobDto>>(dueJobs));
        }

        [HttpGet("standalone")]
        public async Task<ActionResult<ReadJobDto>> GetStandAloneJobs()
        {
            var user = await GetIdentityUser();

            var jobs = await _context.jobs
                .Where(job => job.receiverUserId == user.Id && job.project_id == 0 && job.section_id == 0)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReadJobDto>>(jobs));
        }

        [HttpGet("allboardtaken")]
        public async Task<ActionResult<IEnumerable<ReadJobDto>>> GetAllTakenBoardJobs()
        {
            var user = await GetIdentityUser();

            if (user == null) {
                return NotFound(new { error = "User doesn't exists" });
            }

            var jobs = await _context.taskHasUsers
                .Where(r => r.user_id == user.Id)
                .Select(j => j.job)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ReadJobDto>>(jobs));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(int id) {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var job = await _context.jobs
                .Include(job=>job.section)
                .ThenInclude(section=>section.board)
                .Where(job=>job.Id==id).FirstOrDefaultAsync();

            if (job == null) {
                return NotFound();
            }

            if (job.receiverUserId == user.Id) {
                _context.jobs.Remove(job);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.section.board.project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins.AnyAsync(rel => rel.board_id == job.section.board_id && rel.user_id == user.Id);

            if (!isUserAuthorized) {
                return Unauthorized();
            }

            var jobHasUserRel = await _context.taskHasUsers
                .Where(job => job.job_id == id && job.user_id == user.Id)
                .ToListAsync();

            _context.jobs.Remove(job);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<ReadJobDto>>> GetReceivedJobs()
        {
            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var jobs = await _context.jobs
                .Where(r => r.receiverUserId == user.Id)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ReadJobDto>>(jobs));
        }
        
        [HttpPost("addprojectjob")]
        public async Task<ActionResult> AddProjectJob(CreateJobDto createJobDto)
        {
            var user = await GetIdentityUser();

            var isUserAuthorized = await _context.userHasProjects
               .AnyAsync(rel => rel.project_id == createJobDto.project_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
                return Unauthorized();

            var job = _mapper.Map<Job>(createJobDto);

            job.createUserId = user.Id;

            await _context.jobs.AddAsync(job);
            await _context.SaveChangesAsync();
            await _context.taskHasUsers.AddAsync(
                    new JobHasUsers()
                    {
                        user_id = user.Id,
                        job_id = job.Id
                    }
                );

            return Ok();
        }

        [HttpGet("getprojectjob/{id}")]
        public async Task<ActionResult> GetProjectJobs(int id) {
            var user = await GetIdentityUser();

            var isUserAuthorized = await _context.userHasProjects
               .AnyAsync(rel => rel.project_id == id && rel.user_id == user.Id)
               ||
               await _context.userAssignedProjects
               .AnyAsync(rel => rel.project_id == id && rel.receiver_id== user.Id)
               ;

            if (!isUserAuthorized)
                return Unauthorized();

            var jobs = await _context.jobs
                .Where(job => job.project_id == id
                && job.section_id == 0&&job.receiverUserId==user.Id)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReadJobDto>>(jobs));
        }

        [HttpPost]
        public async Task<ActionResult<ReadJobDto>> CreateJob(CreateJobDto createJobDto) {

            var user = await GetIdentityUser();

            var section = await _context.sections.Include(s=>s.board)
                .FirstOrDefaultAsync(section=>section.Id==createJobDto.section_id);

            if (section == null) {
                return NotFound();
            }

            var job = _mapper.Map<Job>(createJobDto);
            job.createUserId = user.Id;

            if (job.receiverUserId == user.Id && job.section_id == 0 && job.project_id == 0) {

                await _context.jobs.AddAsync(job);
                await _context.SaveChangesAsync();
                await _context.taskHasUsers.AddAsync(
                        new JobHasUsers() {
                            user_id = user.Id,
                            job_id = job.Id
                        }
                    );
                if (createJobDto.tags != null)
                {
                    foreach (string element in createJobDto.tags)
                    {
                        var tag = new Tags()
                        {
                            job_id = job.Id,
                            tagName = element
                        };
                        await _context.tags.AddAsync(tag);
                    }
                }
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetJob", new { id = job.Id }, _mapper.Map<ReadJobDto>(job));
            }

            /*else if (job.receiverUserId == user.Id && (job.section_id == 0 || job.project_id != 0)) {
                return BadRequest();
            }
            else if (job.receiverUserId != null && job.section_id != 0) {
                return BadRequest();
            }*/

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers
                    .AnyAsync(rel => rel.board_id == section.board_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                    .AnyAsync(rel => rel.board_id == section.board_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
                return Unauthorized();

            /*if (job.section_id == 0 && job.receiverUserId != null) {
                var receiverAssignedProject = await _context.userAssignedProjects
                    .AnyAsync(rel => rel.receiver_id == job.receiverUserId);

                var receiverHasProject = await _context.userHasProjects
                    .AnyAsync(rel => rel.user_id == job.receiverUserId);

                if (!receiverAssignedProject && !receiverHasProject) {
                    return Unauthorized();
                }
                var receiverUserExists = await _userManager.FindByIdAsync(job.receiverUserId);
                if (receiverUserExists==null) {
                    return NotFound();
                }
            }

            else if (job.section_id == 0 && job.receiverUserId == null) {
                return BadRequest();
            }*/

            job.section = section;
            job.section_id = section.Id;

            var order = await _context.jobs.CountAsync(job=>job.section_id==section.Id);
            job.order_no = order + 1;

            if (job.section.board.project_id != 0) {
                job.project_id = job.section.board.project_id;
            }

            await _context.jobs.AddAsync(job);
            await _context.SaveChangesAsync();

            if (createJobDto.tags!=null) {
                foreach (string element in createJobDto.tags) {
                    var tag = new Tags()
                    {
                        job_id=job.Id,
                        tagName=element
                    };
                    await _context.tags.AddAsync(tag);
                }
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetJob", new { id = job.Id }, _mapper.Map<ReadJobDto>(job));
        }

        [HttpPost("{id}/takejob")]
        public async Task<ActionResult> TakeJob(int id) {

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var jobFromRepo = await _context.jobs
                .Include(job=>job.section)
                .ThenInclude(section=>section.board)
                .Where(rel=>rel.Id==id).FirstAsync();

            if (jobFromRepo == null)
            {
                return NotFound();
            }

            var userHasTask = await _context.taskHasUsers
                .AnyAsync(rel=>rel.user_id==user.Id&&rel.job_id==id);

            if (jobFromRepo.receiverUserId==user.Id||userHasTask) {
                return BadRequest();
            }

            var isUserAuthorized = await _context.boardHasAdmins
                 .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                  ||
                  await _context.boardHasUsers
                 .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                 ;

            if (!isUserAuthorized) {
                return Unauthorized();
            }

            var rel = new JobHasUsers()
            {
                user_id = user.Id,
                job_id = jobFromRepo.Id
            };

            await _context.taskHasUsers.AddAsync(rel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PartialJobUpdate(int id, JsonPatchDocument<UpdateJobDto> patchDocument) {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var jobFromRepo = await _context.jobs.Include(job=>job.section).FirstOrDefaultAsync(job=>job.Id==id);
            if (jobFromRepo == null)
            {
                return NotFound();
            }

            var jobToPatch = _mapper.Map<UpdateJobDto>(jobFromRepo);
            patchDocument.ApplyTo(jobToPatch, ModelState);
            if (!TryValidateModel(jobToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(jobToPatch, jobFromRepo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        private async Task<User> GetIdentityUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }
    }
}
