using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Dto.JobDto;
using Microsoft.AspNetCore.JsonPatch;

namespace ProjectManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SectionController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public SectionController(ManagementContext context, IMapper mapper, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetSection")]
        public async Task<ActionResult> GetSection(int id) {
            var user = await GetIdentityUser();

            var sectionFromRepo = await _context.sections.Include(section => section.board).FirstOrDefaultAsync(section => section.Id == id);

            if (sectionFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.boardHasUsers
                  .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id &&
                      rel.user_id == user.Id)
                  ||
                  await _context.boardHasAdmins
                  .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            var sectionToSend = _mapper.Map<ReadSectionDto>(sectionFromRepo);

            return Ok(sectionToSend);
        }

        [HttpPost("order")]
        public async Task<ActionResult> UpdateOrder([FromQuery] int order_no, [FromQuery] int section_id)
        {

            var user = await GetIdentityUser();

            var section = await _context.sections.Include(s => s.board)
                .FirstOrDefaultAsync(section => section.Id == section_id);

            if (section == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == section.board.project_id
                    && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers.AnyAsync(rel => rel.board_id == section.board_id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == section.board_id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }
            var total_count = await _context.sections.CountAsync(s => s.board_id == section.board.Id);
            if (order_no > total_count)
            {
                return BadRequest();
            }

            if (order_no >= section.order_no) {
                var sections = await _context.sections.Where(c => c.order_no <= order_no&&c.order_no>section.order_no).ToListAsync();

                foreach (Section s in sections)
                {
                    s.order_no = s.order_no - 1;
                }
            }
            else if (order_no <= section.order_no)
            {
                var sections = await _context.sections.Where(c => c.order_no >= order_no && c.order_no < section.order_no).ToListAsync();

                foreach (Section s in sections)
                {
                    s.order_no = s.order_no + 1;
                }
            }

            section.order_no = order_no;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSection(int id)
        {
            var user = await GetIdentityUser();

            var sectionFromRepo = await _context.sections.Include(section => section.board).FirstAsync(section => section.Id == id);

            if (sectionFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                  .AnyAsync(rel => rel.project_id == sectionFromRepo.board.project_id &&
                      rel.user_id == user.Id)
                  ||
                  await _context.boardHasAdmins
                  .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id && rel.user_id == user.Id)
                  ;

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }

            _context.sections.Remove(sectionFromRepo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<CreateSectionDto>> CreateSection(CreateSectionDto sectionDto)
        {
            var user = await GetIdentityUser();

            var boardFromRepo = await _context.boards.FindAsync(sectionDto.board_id);

            if (boardFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == boardFromRepo.project_id &&
                    rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == boardFromRepo.Id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }
            
            var section = _mapper.Map<Section>(sectionDto);

            var order = await _context.sections.CountAsync(section => section.board_id == sectionDto.board_id);
            section.order_no = order + 1;

            await _context.sections.AddAsync(section);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetSection", new { id = section.Id }, section);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PartialSectionNameUpdate(int id, [FromQuery]string name)
        {
            var user = await GetIdentityUser();
            if (user == null)
            {
                return NotFound(new { error = "User doesn't exists in the current context" });
            }

            var sectionFromRepo = await _context.sections.Include(section => section.board).FirstOrDefaultAsync(section => section.Id == id);
            if (sectionFromRepo == null)
            {
                return NotFound();
            }

            var isUserAuthorized = await _context.userHasProjects
                .AnyAsync(rel => rel.project_id == sectionFromRepo.board.project_id
                    && rel.user_id == user.Id)
                ||
                await _context.boardHasUsers
                .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id && rel.user_id == user.Id)
                ||
                await _context.boardHasAdmins
                .AnyAsync(rel => rel.board_id == sectionFromRepo.board.Id && rel.user_id == user.Id);

            if (!isUserAuthorized)
            {
                return Unauthorized();
            }
            sectionFromRepo.sectionName = name;
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
