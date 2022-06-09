using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class JobUtil : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public JobUtil(ManagementContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult> UpdateOrder([FromQuery]int order_no,[FromQuery]int job_id) {

            var user = await GetIdentityUser();

            var job = await _context.jobs.Include(job => job.section)
                .FirstOrDefaultAsync(job => job.Id == job_id);

            if (job == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.project_id
                    && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers.AnyAsync(rel => rel.board_id == job.section.board_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == job.section.board_id && rel.user_id == user.Id)
                ||
                job.receiverUserId == user.Id;

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var order = await _context.jobs.CountAsync(c => c.section_id == job.section.Id);
            if (order_no>order) {
                return BadRequest();
            }

            if (order_no >= job.order_no)
            {
                var jobs = await _context.jobs.Where(c => c.order_no <= order_no && c.order_no > job.order_no).ToListAsync();

                foreach (Job j in jobs)
                {
                    j.order_no = j.order_no - 1;
                }
            }
            else if (order_no <= job.order_no)
            {
                var jobs = await _context.jobs.Where(c => c.order_no >= order_no && c.order_no < job.order_no).ToListAsync();

                foreach (Job j in jobs)
                {
                    j.order_no = j.order_no + 1;
                }
            }

            job.order_no = order_no;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("tag")]
        public async Task<ActionResult> AddTag([FromBody] CreateTagDto tagDto)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var jobFromRepo = await _context.jobs.Include(j => j.section)
                .Where(j => j.Id == tagDto.job_id).FirstOrDefaultAsync();
            if (jobFromRepo == null)
            {
                return NotFound();
            }

            var tag = _mapper.Map<Tags>(tagDto);
            var activity = new ActivityHistory()
            {
                job_id = jobFromRepo.Id,
                activityType = ActivityTypes.ACTIVITY_TYPE_ADD_TAG,
                detail = string.Format("{0} added a tag with name : '{1}' to this job", user.UserName, tag.tagName)
            };
            await _context.activityHistories.AddAsync(activity);
            await _context.tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("untag/{id}")]
        public async Task<ActionResult> UnTag(int id)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var tag = await _context.tags
                .Include(tag => tag.job)
                .ThenInclude(job=>job.section)
                .Where(tag => tag.Id == id).FirstOrDefaultAsync();

            var jobFromRepo = tag.job;

            if (jobFromRepo == null)
            {
                return NotFound();
            }

            var activity = new ActivityHistory()
            {
                job_id = jobFromRepo.Id,
                activityType = ActivityTypes.ACTIVITY_TYPE_REMOVE_TAG,
                detail = string.Format("{0} removed a tag with name : '{1}' from this job", user.UserName, tag.tagName)
            };


            await _context.activityHistories.AddAsync(activity);
            _context.tags.Remove(tag);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("checklist")]
        public async Task<ActionResult> AddChecklist([FromBody] CreateChecklistDto checklistDto)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var jobFromRepo = await _context.jobs
                .Include(job => job.section)
                .Where(rel => rel.Id == checklistDto.job_id).FirstAsync();
            
            if (jobFromRepo.section_id!=0) {
                if (jobFromRepo == null)
                {
                    return NotFound();
                }
                var isUserAuthorized = await _context.boardHasAdmins
                   .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                    ||
                    await _context.boardHasUsers
                   .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                   ;

                if (!isUserAuthorized)
                {
                    return Unauthorized();
                }
            }

            var checklist = _mapper.Map<CheckList>(checklistDto);

            var activity = new ActivityHistory()
            {
                job_id = jobFromRepo.Id,
                activityType = ActivityTypes.ACTIVITY_TYPE_ADD_CHECKLIST,
                detail = String.Format("{0} added a checklist with text '{1}' to this job", user.UserName, checklist.text)
            };
            await _context.activityHistories.AddAsync(activity);
            await _context.checkLists.AddAsync(checklist);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/attach")]
        public async Task<ActionResult> AddAttachment(IFormFile file, int id)
        {
            if (file == null || file.Length < 0)
            {
                return BadRequest();
            }

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var jobFromRepo = await _context.jobs
                .Include(job => job.section)
                .ThenInclude(section => section.board)
                .Where(rel => rel.Id == id).FirstAsync();

            if (jobFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers
                .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id)
                ||
                (jobFromRepo.receiverUserId==user.Id && jobFromRepo.section_id==0 && jobFromRepo.project_id==0);

            if (!isUserAuthorized)
                return Unauthorized();

            var fileName = Path.GetFileName(file.FileName);
            var fileExtension = Path.GetExtension(fileName);
            var fullFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

            var attachment = new Attachment()
            {
                job_id = id,
                name = fullFileName,
                fileType = fileExtension,
                createdOn = DateTime.Now
            };
            using (var target = new MemoryStream())
            {
                file.CopyTo(target);
                attachment.fileData = target.ToArray();
            }
            await _context.attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("togglechecklist/{id}")]
        public async Task<ActionResult> ToggleChecklist(int id) {
            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var checklist = await _context.checkLists.Include(c => c.job)
                .ThenInclude(j => j.section).ThenInclude(s => s.board)
                .Where(c => c.Id == id).FirstOrDefaultAsync();

            if (checklist == null) {
                return NotFound();
            }

            if (checklist.job.section!=null) {
                var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == checklist.job.project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == checklist.job.section.board.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == checklist.job.section.board.Id);

                if (!isUserAuthorized)
                {
                    return Unauthorized();
                }
            }

            checklist.isSelected = !checklist.isSelected;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("changesection")]
        public async Task<ActionResult> ChangeSection([FromQuery]int job_id,[FromQuery]int new_section_id, [FromQuery] int new_order_no) {
            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var job = await _context.jobs.Include(job=>job.section)
                .ThenInclude(section=>section.board)
                .Where(job=>job.Id==job_id).FirstOrDefaultAsync();

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == job.project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == job.section.board.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == job.section.board.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var order_no = job.order_no;
            var jobsToReorder = await _context.jobs
                .Where(j => j.order_no > order_no&&j.section_id==job.section_id)
                .ToListAsync();

            foreach (Job j in jobsToReorder) {
                j.order_no--;
            }

            var section= await _context.sections
                .FirstOrDefaultAsync(section=>section.Id==new_section_id);

            if (section==null)
                return NotFound();

            var sectionJobCount = await _context.jobs
                .CountAsync(job=>job.section_id==new_section_id);

            var jobsToReorderFromSection = await _context.jobs
                .Where(job=>job.section_id==new_section_id&&
                    job.order_no>=new_order_no
                ).ToListAsync();

            foreach (Job j in jobsToReorderFromSection)
            {
                j.order_no = j.order_no + 1;
            }
            
            job.section = section;
            job.section_id = section.Id;
            job.order_no = new_order_no;

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
