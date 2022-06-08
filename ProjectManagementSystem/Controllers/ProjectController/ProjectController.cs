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
using Microsoft.AspNetCore.JsonPatch;
using ProjectManagementSystem.Dto.BoardReadDto;
using ProjectManagementSystem.Dto.JobDto;

namespace ProjectManagementSystem.Controllers.ProjectController
{
    // TODO : Assign admin to project
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

            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var userHasProjects = await _context.userHasProjects
                .Include(rel=>rel.project)
                .ThenInclude(p=>p.boards)

                .Include(rel=>rel.project)
                .ThenInclude(p=>p.userAssignedProjects)
                .ThenInclude(rel=>rel.receiverUser)

                .Include(rel => rel.project)
                .ThenInclude(p => p.userHasProjects)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.project)
                .ThenInclude(p => p.projectJobs)

                .Include(rel => rel.project)
                .ThenInclude(p => p.teams)

                .FirstOrDefaultAsync(relation=>relation.project_id==id&&relation.user_id==user.Id);

            var userAssignedProjects= await _context.userAssignedProjects
                .Include(rel => rel.project)
                .ThenInclude(p => p.boards)

                .Include(rel => rel.project)
                .ThenInclude(p => p.userAssignedProjects)
                .ThenInclude(rel => rel.receiverUser)

                .Include(rel => rel.project)
                .ThenInclude(p => p.userHasProjects)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.project)
                .ThenInclude(p => p.projectJobs)

                .Include(rel => rel.project)
                .ThenInclude(p => p.teams)
                .Where(relation => relation.receiver_id == user.Id).FirstOrDefaultAsync();

            var result = new Project();

            if (userHasProjects != null)
            {
                result = userHasProjects.project;
            }
            else if(userHasProjects==null&&userAssignedProjects!=null){
                result = userAssignedProjects.project;
            }

            if (userHasProjects==null&&userAssignedProjects==null) {
                return NotFound();
            }
            return Ok(_mapper.Map<ReadProjectDto>(result));
        }

        [HttpGet]
        public async Task<ActionResult<List<ReadProjectDto>>> GetProjects()
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var userHasProjects = await _context.userHasProjects.Include(r=>r.project)
                .ThenInclude(p=>p.boards)
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
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var userAssignedProjectsRel = await _context.userAssignedProjects
                .Include(rel => rel.project)
                .ThenInclude(p => p.boards)

                .Include(rel => rel.project)
                .ThenInclude(p => p.userAssignedProjects)
                .ThenInclude(rel => rel.receiverUser)

                .Include(rel => rel.project)
                .ThenInclude(p => p.userHasProjects)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.project)
                .ThenInclude(p => p.projectJobs)

                .Include(rel => rel.project)
                .ThenInclude(p => p.teams)
                .Where(relation => relation.receiver_id == user.Id).ToListAsync();

            if (userAssignedProjectsRel == null)
            {
                return NotFound();
            }
            var userAssignedProjects = new List<ReadProjectDto>();
            userAssignedProjectsRel.ForEach(rel=>
                    {
                        var project_dto = _mapper.Map<ReadProjectDto>(rel.project);
                        if (rel.assignerUser != null) {
                            project_dto.assigner_user = _mapper.Map<ReadUserDto>(rel.assignerUser);
                        }
                        userAssignedProjects.Add(project_dto);
                    }
                );
            return Ok(userAssignedProjects);
        }

        [HttpPost]
        public async Task<ActionResult<ReadProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            var user = await GetIdentityUser();
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
        [Route("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var project = await _context.userHasProjects.Include(rel => rel.project)
                .Where(rel => rel.user_id == user.Id && rel.project_id == id)
                    .Select(rel => rel.project).FirstAsync();

            if (project==null) {
                return NotFound();
            }

            var projectJobs = await _context.jobs
                .Where(job => job.project_id == id)
                .ToListAsync();

            var projectJobsIds =projectJobs.Select(job => job.Id);

            var projectBoards = await _context.boards
                .Where(board => board.project_id == id)
                .ToListAsync();

            var sections = await _context.sections.Include(s => s.board)
                .Where(section => section.board.project_id == id)
                .ToListAsync();

            var rel = await _context.taskHasUsers
                .Where(rel => projectJobsIds.Contains(rel.job_id))
                .ToListAsync();

            _context.projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Route("assigned/{id}")]
        public async Task<IActionResult> ExitAssignedProject(int id)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists" });
            }
            var projectRel = await _context.userAssignedProjects
                .Where(rel => rel.receiver_id == user.Id && rel.project_id == id)
                .FirstAsync();
            if (projectRel == null)
            {
                return NotFound();
            }
            _context.userAssignedProjects.Remove(projectRel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("assignproject")]
        public async Task<ActionResult> AssignProject([FromQuery] int project_id, string user_email) {

            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var project = await _context.projects.FindAsync(project_id);
            if (project == null)
            {
                return NotFound();
            }
            var targetUser = await _context.users
                .Include(u => u.notifications)
                .Where(u => u.Email == user_email).FirstOrDefaultAsync();
            
            if (targetUser==null) {
                return NotFound();
            }
            var user_id = targetUser.Id;

            var userHasProject = targetUser.notifications.Any(n => n.project_id == project_id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel => rel.receiver_id == user_id && rel.project_id == project_id)
                ||
                await _context.userHasProjects
                .AnyAsync(rel => rel.user_id == user_id && rel.project_id == project_id);
                
            if (userHasProject) {
                return BadRequest();
            }
            targetUser.notifications.Add(new Notification
            {
                owner_user_id=user_id,
                owner_user=targetUser,
                action_type=NotificationConstants.ACTION_TYPE_ASSIGN,
                target_type=NotificationConstants.TARGET_PROJECT,
                sender_user_id=user.Id,
                sender_user = user,
                project_id =project_id,
                project=project
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("{id}/boards")]
        public async Task<ActionResult<List<Board>>> GetBoardsOfProject(int id) {

            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var project = await _context.projects.AnyAsync(p=>p.Id==id);

            if (!project)
            {
                return NotFound();
            }
            var relProjectToUser = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == id && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects.AnyAsync(rel => rel.project_id == id && rel.receiver_id == user.Id);

            if (!relProjectToUser) {
                return Unauthorized();
            }

            var boards = await _context.boards
                .Include(b=>b.sections)
                .Include(b=>b.boardHasAdmins)
                .ThenInclude(b=>b.user)

                .Include(b => b.boardHasTeams)
                .ThenInclude(t=>t.team)

                .Include(b => b.boardHasUsers)
                .ThenInclude(b => b.user)

                .Where(board => board.project_id == id).ToListAsync();
            return Ok(_mapper.Map<List<ReadBoardDto>>(boards));
        }

        [HttpGet("{id}/teams", Name = "GetTeamsOfProject")]
        public async Task<ActionResult<ReadTeamDto>> GetTeamsOfProject(int id)
        {
                    var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == id && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects.AnyAsync(rel => rel.project_id == id && rel.receiver_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }
            var teams = await _context.teams
                .Where(team => team.project_id == id)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReadTeamDto>>(teams));
        }

        [HttpGet("{id}/jobs")]
        public async Task<ActionResult<ReadJobDto>> GetJobsOfProject(int id) {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == id && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel => rel.project_id == id && rel.receiver_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var jobs = await _context.jobs.Where(job => job.project_id == id
            && job.section_id==0
            ).ToListAsync();
            return Ok(_mapper.Map<List<ReadJobDto>>(jobs));
        } 

        [HttpPatch("{id}")]
        public async Task<ActionResult> PartialProjectUpdate(int id, JsonPatchDocument<UpdateProjectDto> patchDocument)
        {
            var projectFromRepo = await _context.projects.FindAsync(id);
            if (projectFromRepo == null) {
                return NotFound();
            }
            var projectToPatch = _mapper.Map<UpdateProjectDto>(projectFromRepo);
            patchDocument.ApplyTo(projectToPatch, ModelState);
            if (!TryValidateModel(projectToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(projectToPatch, projectFromRepo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        private async Task<User> GetIdentityUser() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }
    }
}
