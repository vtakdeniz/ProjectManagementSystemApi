using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models.ProjectElements;
using AutoMapper;
using ProjectManagementSystem.Dto.ProjectDto;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ProjectManagementSystem.Models.UserElements;
using Microsoft.AspNetCore.Authorization;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Dto.UserDto;

namespace ProjectManagementSystem.Controllers.ProjectControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ProjectController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public ProjectController(ManagementContext context,IMapper mapper, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("{id}", Name = "GetProject")]
        public async Task<ActionResult<ReadProjectDto>> GetProject(int id) {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }


            var userHasProjects = await _context.userHasProjects.FirstOrDefaultAsync(relation=>relation.project_id==id&&relation.user_id==user.Id);

            if (userHasProjects==null) {
                return NotFound();
            }

            return Ok(_mapper.Map<ReadProjectDto>(userHasProjects.project));
        }

        [HttpGet]
        public async Task<ActionResult<List<ReadProjectDto>>> GetProjects()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }


            var userHasProjects = await _context.userHasProjects.Include(r=>r.project)
                .Where(relation => relation.user_id == user.Id)
                    .Select(r=>r.project).ToListAsync();
            
            if (userHasProjects == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<IEnumerable<ReadProjectDto>>(userHasProjects));
        }

        [HttpGet]
        [Route("assigned")]
        public async Task<ActionResult<List<ReadProjectDto>>> GetAssignedProjects()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var userAssignedProjectsRel = await _context.userAssignedProjects.Include(r => r.project).Include(r=>r.assignerUser)
                .Where(relation => relation.receiver_id == userId).ToListAsync();

            if (userAssignedProjectsRel == null)
            {
                return NotFound();
            }

            var userAssignedProjects = new List<ReadProjectDto>();

            userAssignedProjectsRel.ForEach(rel=>
                    {
                        var project_dto = _mapper.Map<ReadProjectDto>(rel.project);
                        project_dto.assigner_user = _mapper.Map<ReadUserDto>(rel.assignerUser);
                        userAssignedProjects.Add(project_dto);
                    }
                );
           
            return Ok(userAssignedProjects);
        }

        [HttpPost]
        [Route("addproject")]
        public async Task<ActionResult<ReadProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var project = _mapper.Map<Project>(createProjectDto);

            var userHasProjects = new UserHasProjects
            {
                user = user,
                user_id = user.Id,
                project = project,
                project_id = project.Id
            };

            await _context.projects.AddAsync(project);
            await _context.userHasProjects.AddAsync(userHasProjects);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProject", new { id = project.Id }, _mapper.Map<ReadProjectDto>(project));

        }

        [HttpDelete]
        [Route("deleteproject/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var project = await _context.projects.FindAsync(id);

            if (project==null) {
                return NotFound();
            }

            _context.projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost]
        [Route("assignproject")]
        public async Task<ActionResult> AssignProject([FromQuery] int projectid, string targetid) {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }

            var project = await _context.projects.FindAsync(projectid);

            if (project == null)
            {
                return NotFound();
            }

            var targetUser = await _context.Users.Include(u => u.notifications).FirstOrDefaultAsync(u => u.Id == targetid);
            
            if (targetUser == null) {
                return NotFound();
            }

            var userHasProject=targetUser.notifications.Any(n => n.project_id == projectid);

            if (userHasProject) {
                return BadRequest();
            }


            targetUser.notifications.Add(new Notification
            {
                owner_user_id=targetid,
                action_type=NotificationConstants.ACTION_TYPE_ASSIGN,
                target_type=NotificationConstants.TARGET_PROJECT,
                sender_user_id=userId,
                project_id=projectid
            });

            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
