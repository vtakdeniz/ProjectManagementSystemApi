using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.BoardReadDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Dto.JobDto;

namespace ProjectManagementSystem.Controllers.BoardController
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class BoardController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public BoardController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetBoard")]
        public async Task<ActionResult<ReadBoardDto>> GetBoard(int id) {
            var user = await GetIdentityUser();

            var board= await _context.boards

                .Include(board => board.boardHasAdmins)
                .ThenInclude(rel => rel.user)

                .Include(rel=>rel.sections)
                .ThenInclude(s=>s.jobs)
                .ThenInclude(j=>j.jobHasUsers)
                .ThenInclude(r=>r.user)

                .Include(board=>board.boardHasTeams)
                .ThenInclude(rel=>rel.team)

                .Include(board=>board.boardHasUsers)
                .ThenInclude(rel=>rel.user)
                .Where(board => board.Id == id).FirstOrDefaultAsync();
            
            if (board == null) {
                return NotFound();
            }

            var projectRel = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == board.project_id && rel.user_id == user.Id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel=>rel.receiver_id==user.Id&&rel.project_id==board.project_id)
                ||
                board.project_id==0;

            if (!projectRel) {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ReadBoardDto>(board));
        }

        [HttpGet]
        public async Task<ActionResult<ReadBoardDto>> GetBoards()
        {
            // TODO : Fix collision
            var user = await GetIdentityUser();

            var userBoards = await _context.boardHasUsers
                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasAdmins)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasTeams)
                .ThenInclude(rel => rel.team)

                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasUsers)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.board)
                .ThenInclude(rel=>rel.sections)

                .Where(rel => rel.user_id == user.Id)
                .Select(rel=>rel.board)
                .ToListAsync();

            var adminBoards = await _context.boardHasAdmins
                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasAdmins)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasTeams)
                .ThenInclude(rel => rel.team)

                .Include(rel => rel.board)
                .ThenInclude(board => board.boardHasUsers)
                .ThenInclude(rel => rel.user)

                .Include(rel => rel.board)
                .ThenInclude(rel => rel.sections)

                .Where(rel => rel.user_id == user.Id)
                .Select(rel => rel.board).ToListAsync();

            var result=userBoards
                .Concat(adminBoards)
                .GroupBy(board=>board.Id)
                .Select(board=>board.First())
                .ToList();

            if (userBoards == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<IEnumerable <ReadBoardDto>> (result));
        }

        [HttpGet("date")]
        public async Task<ActionResult> GetBoardsByDate([FromQuery] string timeunit, [FromQuery] int span)
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
            else {
                seconds = 1440;
            }

            var user = await GetIdentityUser();

            var userBoards = await _context.boardHasUsers
                .Where(rel => rel.user_id == user.Id)
                .Select(rel => rel.board_id)
                .ToListAsync();


            var boards = await _context.boards
                .Where(board =>
                (userBoards.Contains(board.Id))
                )
                .ToListAsync();

            var dueBoards = new List<Board>();
            var nowMinutes = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes;
            foreach (Board b in boards)
            {
                var boardMinutes = TimeSpan.FromTicks(b.endDate.Ticks).TotalMinutes;
                var result = (boardMinutes - nowMinutes) / seconds;
                if (result < span && b.endDate.Date>DateTime.Now)
                {
                    dueBoards.Add(b);
                }
            }

            return Ok(_mapper.Map<List<ReadBoardDto>>(dueBoards));
        }

        [HttpPost("standalone")]
        public async Task<ActionResult<ReadBoardDto>> CreateStandaloneBoard([FromBody] CreateBoardDto boardDto)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            if (boardDto.project_id!=0||boardDto.team_ids!=null||boardDto.user_ids!=null) {
                return BadRequest();
            }

            var board = _mapper.Map<Board>(boardDto);
            await _context.boards.AddAsync(board);
            await _context.SaveChangesAsync();

            var rel = new BoardHasAdmins
            {
               user_id=user.Id,
               board_id=board.Id
            };

            await _context.boardHasAdmins.AddAsync(rel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBoard", new { id = board.Id }, _mapper.Map<ReadBoardDto>(board));
        }

        [HttpPost]
        public async Task<ActionResult<ReadBoardDto>> CreateProjectBoard([FromBody]CreateBoardDto boardDto) {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var project_id = boardDto.project_id;
            var projectFromRepo = await _context.projects.FindAsync(project_id);
            if (projectFromRepo == null) {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!isUserAuthorized) 
                return Unauthorized();

            var board = _mapper.Map<Board>(boardDto);
            await _context.boards.AddAsync(board);
            board.project = projectFromRepo;

            if (boardDto.team_ids == null && boardDto.user_ids == null)
            {

                await _context.SaveChangesAsync();

                await _context.boardHasUsers.AddAsync(
                    new BoardHasUsers
                {
                    board_id = board.Id,
                    user_id = user.Id
                });

                var boardHasAdminsRelation = new BoardHasAdmins
                {
                    board_id = board.Id,
                    user_id = user.Id
                };
                await _context.boardHasAdmins.AddAsync(boardHasAdminsRelation);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetBoard", new { id = board.Id }, _mapper.Map<ReadBoardDto>(board));
            }
            else
            {
                bool? areAllDtoUsersValid = boardDto.user_ids?
                   .All(id => _context.userHasProjects.Any(rel => rel.user_id == id &&
                        rel.project_id == board.project_id));

                if (areAllDtoUsersValid != null&&!areAllDtoUsersValid.Value)
                    return BadRequest();

                bool? areAllDtoTeamsValid = boardDto.team_ids?
                   .All(id => _context.teams.Any(team => team.Id == id &&
                       team.project_id == board.project_id));

                if (areAllDtoTeamsValid != null && !areAllDtoTeamsValid.Value)
                    return BadRequest();
            }

            await _context.SaveChangesAsync();
            var boardHasAdminsRel = new BoardHasAdmins
            {
                board_id = board.Id,
                user_id = user.Id
            };
            await _context.boardHasAdmins.AddAsync(boardHasAdminsRel);

            if (boardDto.team_ids!=null) {
                var projectTeamRel = await _context.teams
                .Where(team => team.project_id == boardDto.project_id
                && boardDto.team_ids.Any(id => id == team.Id))
                .Include(team => team.teamHasUsers)
                .ThenInclude(rel => rel.user)
                .ToListAsync();
                foreach (Team team in projectTeamRel)
                {
                    await _context.boardHasTeams.AddAsync(
                            new BoardHasTeams
                            {
                                team_id = team.Id,
                                board_id = board.Id
                            }
                        );
                    team.teamHasUsers.ForEach(async rel => {
                        await _context.boardHasUsers.AddAsync(
                            new BoardHasUsers
                            {
                                board_id = board.Id,
                                user_id = rel.user_id
                            }
                        );
                    });
                }
            }
            if (boardDto.user_ids != null) {
                var projectUserRel = await _context.userAssignedProjects
                .Where(rel => boardDto.user_ids.Any(id => id == rel.receiver_id) &&
                    rel.project_id == board.project_id)
                .Include(rel => rel.receiverUser)
                .Select(rel => rel.receiverUser)
                .ToListAsync();
                projectUserRel.ForEach(async user => {
                    await _context.boardHasUsers.AddAsync(
                            new BoardHasUsers
                            {
                                user_id = user.Id,
                                board_id = board.Id
                            }
                        );
                });
            }
           
            await _context.boardHasUsers.AddAsync(
                new BoardHasUsers
            {
                board_id = board.Id,
                user_id = user.Id
            });
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetBoard",new { id=board.Id }, _mapper.Map<ReadBoardDto>(board));
        }

        // TODO : Change method update to patch
        [HttpPut]
        public async Task<ActionResult<Board>> UpdateBoard([FromBody] Board board)
        {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.AnyAsync(b=>b.Id==board.Id);

            if (!boardFromRepo)
            {
                return NotFound();
            }

            var project_id = board.project_id;
            var projectRel = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!projectRel)
            {
                return Unauthorized();
            }

            _context.Update(board);
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<ReadBoardDto>(board));
        }

        [HttpPost("assignadmin")]
        public async Task<ActionResult> AssignAdmin([FromQuery]int board_id,string user_email)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var boardFromRepo = await _context.boards.FindAsync(board_id);
            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var targetUser = await _context.Users.Include(u => u.notifications)
                .FirstOrDefaultAsync(u => u.Email == user_email);

            if (targetUser == null)
            {
                return NotFound();
            }

            var user_id = targetUser.Id;

            var isUserAuthorized = await _context.boardHasAdmins
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == boardFromRepo.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var targetUserAdminBoard = targetUser.notifications.Any(n => n.board_id == board_id)
                ||
                await _context.boardHasAdmins.AnyAsync(rel => rel.board_id == board_id && rel.user_id == user_id);

            var targetUserHasBoard = await _context.boardHasUsers.AnyAsync(rel => rel.board_id == board_id && rel.user_id == user_id);

            if (targetUserAdminBoard || targetUserHasBoard)
            {
                return BadRequest();
            }
            targetUser.notifications.Add(new Notification
            {
                owner_user_id = user_id,
                owner_user=targetUser,
                action_type = NotificationConstants.ACTION_TYPE_ASSIGN_ADMIN,
                target_type = NotificationConstants.TARGET_BOARD,
                sender_user_id = user.Id,
                sender_user=user,
                board_id = board_id,
                board=boardFromRepo
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBoard(int id) {

            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var project_id = boardFromRepo.project_id;
            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == id && rel.user_id == user.Id)
                ;

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var boardJobs = await _context.jobs
                .Include(job => job.section)
                .ThenInclude(section => section.board)
                .Where(job => job.section.board.Id == id)
                .ToListAsync();

            var boardJobsIds = boardJobs.Select(job => job.Id);

            var section = await _context.sections
                .Where(section => section.board_id == id).ToListAsync();

            var jobHasUserRel = await _context.taskHasUsers
                .Where(rel => boardJobsIds.Contains(rel.job_id)
                ).ToListAsync();

            _context.boards.Remove(boardFromRepo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpPost("adduser")]
        public async Task<ActionResult> AddUserToBoardFromProject([FromQuery]string user_id, [FromQuery]int board_id) {

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var userToAdd = await _userManager.FindByIdAsync(user_id);
            var boardFromRepo = await _context.boards.FindAsync(board_id);

            if (userToAdd == null||boardFromRepo==null)
            {
                return NotFound();
            }

            var project_id = boardFromRepo.project_id;

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var isUserInBoard = await _context.boardHasUsers
                .AnyAsync(rel => rel.board_id == boardFromRepo.Id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == boardFromRepo.Id && rel.user_id == user.Id);

            var isUserInProject = await _context.userHasProjects
                .AnyAsync(rel=>rel.user_id==user_id&&project_id==boardFromRepo.project_id)
                ||
                await _context.userAssignedProjects
                .AnyAsync(rel => rel.receiver_id == user_id && project_id == boardFromRepo.project_id);

            if (isUserInBoard && !isUserInProject) {
                return BadRequest();
            }

            var rel = new BoardHasUsers()
            {
                user_id=user_id,
                board_id=board_id
            };

            await _context.boardHasUsers.AddAsync(rel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("removeuser")]
        public async Task<ActionResult> RemoveUserFromBoard([FromQuery] string user_id, [FromQuery] int board_id)
        {
            var user = await GetIdentityUser();
            if (user == null)
                return NotFound(new { error = "User doesn't exists in the current context" });
            var userToAdd = await _userManager.FindByIdAsync(user_id);
            var board = await _context.boards.FindAsync(board_id);

            if (userToAdd == null || board == null)
                return NotFound();

            var project_id = board.project_id;
            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
                return Unauthorized();

            var isUserInBoard = await _context.boardHasUsers
                .AnyAsync(rel => rel.board_id == board.Id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == board.Id && rel.user_id == user.Id);

            if (isUserInBoard)
                return BadRequest();

            var rel = await _context.boardHasUsers
                .FirstAsync(rel=>rel.board_id==board_id&&rel.user_id==user_id);

            _context.boardHasUsers.Remove(rel);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assignboard")]
        public async Task<ActionResult> AssignBoard([FromQuery] int board_id, [FromQuery] string email)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }
            var boardFromRepo = await _context.boards.FindAsync(board_id);
            if (boardFromRepo == null)
            {
                return NotFound();
            }
            var targetUser = await _context.Users.Include(u => u.notifications)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (targetUser == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.boardHasAdmins
                .AnyAsync(rel=>rel.user_id==user.Id&&rel.board_id==boardFromRepo.Id);

            if (!isUserAuthorized) {
                return Unauthorized();
            }

            var userHasBoard = targetUser.notifications.Any(n => n.board_id == board_id)
                ||
                await _context.boardHasUsers.AnyAsync(rel => rel.board_id == board_id && rel.user_id == targetUser.Id)
                ||
                await _context.boardHasAdmins.AnyAsync(rel => rel.board_id == board_id && rel.user_id == targetUser.Id);
            if (userHasBoard)
            {
                return BadRequest();
            }
            targetUser.notifications.Add(new Notification
            {
                owner_user_id = targetUser.Id,
                owner_user=targetUser,
                action_type = NotificationConstants.ACTION_TYPE_ASSIGN,
                target_type = NotificationConstants.TARGET_BOARD,
                sender_user_id = user.Id,
                sender_user=user,
                board_id = board_id,
                board=boardFromRepo
            });
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PartialBoardUpdate(int id, JsonPatchDocument<UpdateBoardDto> patchDocument)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var isUserAuthorized = await _context.boardHasAdmins
                .AnyAsync(rel => rel.user_id == user.Id && rel.board_id == id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var boardFromRepo = await _context.boards.FindAsync(id);
            if (boardFromRepo == null)
            {
                return NotFound();
            }
            var boardToPatch = _mapper.Map<UpdateBoardDto>(boardFromRepo);
            patchDocument.ApplyTo(boardToPatch, ModelState);
            if (!TryValidateModel(boardToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(boardToPatch, boardFromRepo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/jobs")]
        public async Task<ActionResult> GetJobsOfBoard(int id){

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var boardFromRepo = await _context.boards.FindAsync(id);

            var isUserAuthorized = await _context.userAssignedProjects
                .AnyAsync(rel=>rel.receiver_id==user.Id&&rel.project_id==id)
                ||
                await _context.userHasProjects
                .AnyAsync(rel => rel.user_id == user.Id && rel.project_id == id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var jobs = await _context.jobs.Include(job => job.section)
                .Where(job => job.section.board_id == id)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReadJobDto>>(jobs));
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
