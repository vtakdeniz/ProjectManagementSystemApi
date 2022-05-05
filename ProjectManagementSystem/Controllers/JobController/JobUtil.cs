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

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpPost("{id}/tag")]
        public async Task<ActionResult> AddTag(int id, [FromBody] CreateTagDto tagDto)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var jobFromRepo = await _context.jobs.FindAsync(id);
            if (jobFromRepo == null)
            {
                return NotFound();
            }
            var isUserAuthorized = await _context.boardHasAdmins
              .AnyAsync(rel => rel.board_id == jobFromRepo.section.board_id && rel.user_id == user.Id);
            if (!isUserAuthorized)
            {
                return Unauthorized();
            }
            var tag = _mapper.Map<Tags>(tagDto);
            var activity = new ActivityHistory()
            {
                job_id = jobFromRepo.Id,
                activityType = ActivityTypes.ACTIVITY_TYPE_ADD_TAG,
                detail = String.Format("{0} added a tag with name : '{1}' to this job", user.UserName, tag.tagName)
            };
            await _context.activityHistories.AddAsync(activity);
            await _context.tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/checklist")]
        public async Task<ActionResult> AddChecklist(int id, [FromBody] CreateChecklistDto checklistDto)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var jobFromRepo = await _context.jobs
                .Include(job => job.section)
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
               ;

            if (!isUserAuthorized)
            {
                return Unauthorized();
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
