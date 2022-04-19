using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.ReadBoardDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace ProjectManagementSystem.Controllers.TeamController
{   

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class TeamController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public TeamController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetTeam")]
        public async Task<ActionResult<ReadTeamDto>> GetTeam(int id) {
            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var team = await _context.teams.FindAsync(id);

            if (team == null) {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(relation => relation.project_id == team.project_id
                    && relation.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ReadTeamDto>(team));
        }

        [SwaggerOperation(Summary = "Create a team for a project")]
        [HttpPost]
        public async Task<ActionResult<ReadTeamDto>> CreateTeam(CreateTeamDto teamDto) {

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var userHasProjectsRel = await _context.userHasProjects
               .AnyAsync(relation => relation.project_id == teamDto.project_id
                   && relation.user_id == user.Id);

            if(!userHasProjectsRel)
            {
                return Unauthorized();
            }

            var team = _mapper.Map<Team>(teamDto);

            await _context.teams.AddAsync(team);

            if (teamDto.user_ids==null) {
                await _context.SaveChangesAsync();
            }
            else 
            {
                bool areAllDtoUsersValid = teamDto.user_ids
                   .All(id => _context.userAssignedProjects.Any(rel => rel.receiver_id == id &&
                        rel.project_id == teamDto.project_id));

                if (!areAllDtoUsersValid)
                {
                    return BadRequest();
                }

                await _context.SaveChangesAsync();

                var usersFromRepo = await _context.userAssignedProjects
                    .Where(rel => teamDto.user_ids.Any(id => id == rel.receiver_id) &&
                        rel.project_id == teamDto.project_id)
                    .Include(rel => rel.receiverUser)
                    .Select(rel => rel.receiverUser)
                    .ToListAsync();

                foreach (User userFromRepo in usersFromRepo)
                {
                    await _context.teamHasUsers.AddAsync(
                            new TeamHasUsers {
                                team_id=team.Id,
                                user_id=userFromRepo.Id
                            }
                        );
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeam", new { id = team.Id }, _mapper.Map<ReadTeamDto>(team));
        }

        [SwaggerOperation(Summary = "Remove a team from the board")]
        [HttpDelete("delete")]
        public async Task<ActionResult> RemoveTeamFromBoard([FromQuery] int team_id, [FromQuery] int board_id)
        {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(board_id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == boardFromRepo.project_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == boardFromRepo.Id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var boardHasTeamsRel = await _context.boardHasTeams
                .FirstAsync(rel => rel.team_id == team_id && rel.board_id == board_id);

            _context.boardHasTeams.Remove(boardHasTeamsRel);
            await _context.SaveChangesAsync();

            return Ok();
        }



        [SwaggerOperation(Summary = "Add a team that is in the same project with the board, to the board")]
        [HttpPost("add")]
        public async Task<ActionResult> AddTeamToBoard([FromQuery] int team_id, [FromQuery] int board_id)
        {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(board_id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == boardFromRepo.project_id &&
                    rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var relsInBoardProject = await _context.teamHasUsers
                .Include(rel => rel.team)
                .Include(rel => rel.user)
                .Where(rel => rel.team.project_id == boardFromRepo.project_id &&
                    rel.team.Id == team_id)
                .ToListAsync();

            if (relsInBoardProject == null)
            {
                return BadRequest();
            }
            await _context.boardHasTeams.AddAsync(
                    new BoardHasTeams
                    {
                        board_id = board_id,
                        team_id = team_id
                    }
                );
            var usersFromTeams = relsInBoardProject
                    .Select(rel => rel.user).ToList();

            usersFromTeams.ForEach(async user =>
            {
                await _context.boardHasUsers.AddAsync(
                        new BoardHasUsers
                        {
                            user_id = user.Id,
                            board_id = board_id
                        }
                    );
            });
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
