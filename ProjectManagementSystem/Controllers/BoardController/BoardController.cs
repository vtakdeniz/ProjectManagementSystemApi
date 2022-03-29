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

            var boardList = await _context.boards

                .Include(board => board.boardHasAdmins)
                .ThenInclude(rel => rel.user)

                .Include(board=>board.boardHasTeams)
                .ThenInclude(rel=>rel.team)

                .Include(board=>board.boardHasUsers)
                .ThenInclude(rel=>rel.user)
                .Where(board => board.Id == id).ToListAsync();
            

            if (boardList == null) {
                return NotFound();
            }

            var board = boardList[0];

            var projectRel = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == board.project_id && rel.user_id == user.Id);

            if (!projectRel) {
                return Unauthorized();
            }

            return Ok(_mapper.Map<ReadBoardDto>(board));
        }

        [HttpPost]
        public async Task<ActionResult<ReadBoardDto>> PostBoard([FromBody]CreateBoardDto boardDto) {

            var user = await GetIdentityUser();

            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var project_id = boardDto.project_id;
            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!isUserAuthorized) {
                return Unauthorized();
            }

            var board = _mapper.Map<Board>(boardDto);
            await _context.boards.AddAsync(board);

            if (boardDto.team_ids == null && boardDto.user_ids == null)
            {
                await _context.SaveChangesAsync();

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

                if (areAllDtoUsersValid!=null||!areAllDtoUsersValid.Value)
                {
                    return BadRequest();
                }

                bool? areAllDtoTeamsValid = boardDto.team_ids?
                   .All(id => _context.teams.Any(team => team.Id == id &&
                       team.project_id == board.project_id));

                if (areAllDtoTeamsValid!=null||!areAllDtoUsersValid.Value)
                {
                    return BadRequest();
                }
            }

            var boardHasAdminsRel = new BoardHasAdmins
            {
                board_id = board.Id,
                user_id = user.Id
            };

            await _context.boardHasAdmins.AddAsync(boardHasAdminsRel);

            await _context.SaveChangesAsync();

            if (boardDto.team_ids!=null) {
                var projectTeamRel = await _context.teams
                .Where(team => team.project_id == boardDto.project_id
                && boardDto.team_ids.Any(id => id == team.Id)
                )
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBoard(int id) {

            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var project_id = boardFromRepo.project_id;
            var isUserAuthorized = await _context.userHasProjects.AnyAsync(rel => rel.project_id == project_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            _context.boards.Remove(boardFromRepo);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("addteam")]
        public async Task<ActionResult> AddTeamToBoard([FromQuery] int team_id,[FromQuery] int board_id) {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(board_id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel=>rel.project_id==boardFromRepo.project_id &&
                    rel.user_id==user.Id);

            if (!isUserAuthorized) {
                return Unauthorized();
            }

            var relsInBoardProject = await _context.teamHasUsers
                .Include(rel => rel.team)
                .Include(rel=>rel.user)
                .Where(rel => rel.team.project_id == boardFromRepo.project_id &&
                    rel.team.Id==team_id)
                .ToListAsync();

            if (relsInBoardProject == null)
            {
                return BadRequest();
            }

            await _context.boardHasTeams.AddAsync(
                    new BoardHasTeams {
                        board_id=board_id,
                        team_id=team_id
                    }
                );

            var usersFromTeams = relsInBoardProject
                    .Select(rel => rel.user).ToList();

            usersFromTeams.ForEach(async user =>
            {
                await _context.boardHasUsers.AddAsync(
                        new BoardHasUsers {
                             user_id=user.Id,
                             board_id=board_id
                        }
                    );
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("addsection")]
        public async Task<ActionResult<CreateSectionDto>> AddSection(CreateSectionDto sectionDto) {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(sectionDto.board_id);

            if (boardFromRepo == null) {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == boardFromRepo.project_id &&
                    rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var section = _mapper.Map<Section>(sectionDto);

            await _context.sections.AddAsync(section);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("addsection",sectionDto);
        }

        [HttpDelete("removesection/{id}")]
        public async Task<ActionResult> DeleteSection(int id) {
            var user = await GetIdentityUser();

            var sectionFromRepo = await _context.sections.Include(section=>section.board).FirstAsync(section=>section.Id==id);
            
            if (sectionFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == sectionFromRepo.board.project_id &&
                    rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            _context.sections.Remove(sectionFromRepo);
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
