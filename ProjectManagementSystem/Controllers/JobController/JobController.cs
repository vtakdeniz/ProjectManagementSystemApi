using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
                .FindAsync(id);

            if (job == null) {
                return NotFound();
            }

            if (job.receiverUserId==user.Id) {
                return Ok(_mapper.Map<ReadJobDto>(job));
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.project_id
                    && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel => rel.project_id == job.project_id
                    && rel.receiver_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ReadJobDto>(job));
        }

        // TODO : Get jobs based on board id
        [HttpGet("board")]
        public async Task<ActionResult<IEnumerable<ReadJobDto>>> GetBoardJobs()
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
                .Where(job=>job.Id==id).FirstAsync();

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

        [HttpPost]
        public async Task<ActionResult<ReadJobDto>> CreateJob(CreateJobDto createJobDto) {

            var user = await GetIdentityUser();

            var section = await _context.sections.FirstOrDefaultAsync(section=>section.Id==createJobDto.section_id);

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

            else if (job.receiverUserId == user.Id && (job.section_id == 0 || job.project_id != 0)) {
                return BadRequest();
            }
            else if (job.receiverUserId!=null&&job.section_id != 0) {
                return BadRequest();
            }

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

            if (job.section_id == 0 && job.receiverUserId != null) {
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
            }

            job.section = section;
            job.section_id = section.Id;
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
